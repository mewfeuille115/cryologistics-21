# ADR-10: Publicación Garantizada de Eventos de Integración (Outbox / Inbox)

**Estado:** Aceptado  
**Fecha:** 2026-05-01  
**Autores:** Equipo CryoLogistics

---

## Contexto

El sistema aplica DDD con dos categorías distintas de eventos:

- **Eventos de dominio** — ocurren dentro del bounded context `Shipment Monitoring`. Viven exclusivamente en el Event Store (PostgreSQL). No salen del BC. Ejemplos: `TemperatureRecorded`, `ExcursionDetected`, `ReleaseSigned`.
- **Eventos de integración** — notifican cambios relevantes a otros bounded contexts o sistemas externos. Ejemplos: `ReleaseSignedIntegrationEvent` hacia un ERP, `ShipmentClosedIntegrationEvent` hacia un sistema de facturación.

El problema es el **dual-write**: la API publica a Kafka inmediatamente después del `INSERT` en PostgreSQL. Si la API falla en ese instante, el evento de integración se pierde sin dejar rastro. No existe una transacción distribuida que abarque PostgreSQL y Kafka.

Este problema **no aplica al Projector** (cubierto por ADR-06 mediante guardia `sequence_number`), sino exclusivamente al camino de publicación de eventos de integración desde la API hacia consumidores externos.

---

## Opciones evaluadas

| Estrategia                         | Mecanismo                                                                                      | Trade-offs                                                                          | Decisión      |
| ---------------------------------- | ---------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- | ------------- |
| **Dual-write directo**             | `INSERT event_store` + `kafka.Produce()` en la misma operación, sin coordinación               | Simple. Pérdida silenciosa si Kafka falla después del commit SQL.                   | ❌ Descartado  |
| **Outbox + relay (poller)**        | `INSERT outbox` en la misma transacción SQL. Un poller publica a Kafka y marca como procesado. | Garantía de entrega. Sin dependencia de CDC. Añade latencia de polling (≤ 1s).      | ✅ **Elegido** |
| **Outbox + CDC (Debezium)**        | Change Data Capture lee el WAL de PostgreSQL y publica a Kafka automáticamente.                | Latencia mínima. Añade Debezium como componente operacional adicional.              | ⚠️ Fase futura |
| **Inbox en consumidores externos** | Cada BC receptor inserta en una tabla `inbox` antes de procesar, garantizando idempotencia.    | Idempotencia robusta en el receptor. Requiere implementación en cada BC consumidor. | ✅ **Elegido** |

---

## Decisión

Se implementa el patrón **Transactional Outbox** en la API y el patrón **Inbox** en cada bounded context consumidor de eventos de integración.

El **Projector** queda explícitamente fuera de este patrón — su idempotencia está resuelta por diseño mediante `sequence_number` (ADR-06), y añadir un Inbox SQL introduciría latencia de disco incompatible con NFR-3 (< 200ms).

---

## Mecanismo: Outbox (API → Kafka)

### Escritura atómica

La API inserta el evento de dominio y el evento de integración pendiente en la **misma transacción SQL**:

```sql
BEGIN;

INSERT INTO event_store (aggregate_id, event_type, sequence_number, payload, hash, ...)
VALUES (...);

INSERT INTO outbox (id, type, topic, content, occurred_on_utc)
VALUES (gen_random_uuid(), 'ReleaseSignedIntegrationEvent', 'integration-events', '{"type":"ReleaseSignedIntegrationEvent",...}', NOW());

COMMIT;
```

Si la transacción falla, ninguna fila se escribe. Si tiene éxito, ambas quedan atómicamente en PostgreSQL.

### Relay (publicación a Kafka)

Un hosted service consulta periódicamente la tabla `outbox`:

```sql
SELECT id, type, topic, content
FROM outbox
WHERE processed_on_utc IS NULL
ORDER BY occurred_on_utc
LIMIT 100;
```

Por cada fila, publica a Kafka y marca como procesada:

```sql
UPDATE outbox SET processed_on_utc = NOW() WHERE id = $1;
```

El intervalo de polling objetivo es **≤ 1 segundo**, aceptable para eventos de integración (no son monitoreo en tiempo real).

---

## Mecanismo: Inbox (Kafka → BC consumidor)

Cada bounded context externo inserta el mensaje recibido en su tabla `inbox` antes de procesarlo:

```sql
INSERT INTO inbox (id, type, content, occurred_on_utc)
VALUES ($1, $2, $3, $4)
ON CONFLICT (id) DO NOTHING;  -- guardia de idempotencia
```

Si `id` ya existe (reentrega de Kafka), la inserción es no-op y el procesamiento se omite.

---

## Schema de referencia

```sql
-- Tabla outbox (BC Shipment Monitoring)
CREATE TABLE outbox (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type             TEXT        NOT NULL,   -- tipo del evento (ej. 'BatchReleasedIntegrationEvent')
    topic            TEXT        NOT NULL,   -- topic Kafka destino (ej. 'integration-events')
    content          JSONB       NOT NULL,   -- payload serializado del evento
    occurred_on_utc  TIMESTAMPTZ NOT NULL,   -- timestamp original del evento (UTC/NTP)
    processed_on_utc TIMESTAMPTZ,            -- cuando el relay publicó a Kafka exitosamente
    error            TEXT                    -- mensaje de error si el relay falló
);

CREATE INDEX idx_outbox_unprocessed ON outbox (occurred_on_utc)
WHERE processed_on_utc IS NULL;

-- Tabla inbox (cada BC consumidor)
CREATE TABLE inbox (
    id               UUID PRIMARY KEY,       -- event_id del mensaje recibido (guardia de idempotencia)
    type             TEXT        NOT NULL,   -- tipo del evento
    content          JSONB       NOT NULL,   -- payload serializado del evento
    occurred_on_utc  TIMESTAMPTZ NOT NULL,   -- timestamp original del evento en el BC productor
    processed_on_utc TIMESTAMPTZ,            -- cuando este BC procesó el evento
    error            TEXT                    -- mensaje de error si el procesamiento falló
);
```

### Correspondencia con el modelo .NET

```csharp
// Mismo schema para OutboxMessage e InboxMessage
public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; }       // nombre del tipo del evento de integración
    public string Topic { get; init; }      // topic Kafka destino (solo Outbox)
    public string Content { get; init; }    // JSON serializado
    public DateTime OccurredOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; init; }
    public string? Error { get; init; }
}
```

> **Nota:** `topic` no existe en `InboxMessage` — el consumidor ya conoce su propio topic. La columna `error` permite al relay del Outbox y al processor del Inbox registrar fallos sin perder el mensaje, habilitando reintentos y alertas sobre filas con `error IS NOT NULL`.

---

## Consecuencias

**Positivas:**
- Publicación garantizada: ningún evento de integración se pierde silenciosamente.
- Sin dependencias externas adicionales (el relay es un hosted service .NET, sin Debezium).
- La tabla `outbox` sirve como log de auditoría de todos los eventos de integración publicados.
- El Inbox garantiza idempotencia en consumidores sin acoplamiento al productor.

**Negativas:**
- Latencia de integración de hasta ~1 segundo (polling interval). Aceptable para integración; inaceptable para monitoreo en tiempo real (de ahí que el Projector quede fuera).
- La tabla `outbox` crece si el relay falla; requiere alerta sobre filas con `processed_at IS NULL` con lag alto.

**Neutras:**
- El relay por poller puede reemplazarse por Debezium CDC sin cambios en los consumidores.

---

## Relación con otros ADRs

| ADR    | Relación                                                                                    |
| ------ | ------------------------------------------------------------------------------------------- |
| ADR-01 | El `outbox` comparte la misma transacción SQL que el `event_store`. Mismo PostgreSQL.       |
| ADR-02 | El relay publica al topic `integration-events` en Kafka (separado de `shipment-events`).    |
| ADR-06 | El Projector usa guardia `sequence_number`, **no** Inbox. Caminos explícitamente separados. |
