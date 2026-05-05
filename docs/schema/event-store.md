# Event Store — Schema y Convenciones

> [← README](../../README.md) | [Arquitectura — Contenedores](../architecture/architecture.md#c4-nivel-2-contenedores)

---

## DDL — PostgreSQL

```sql
CREATE TABLE event_store (
    id               BIGSERIAL        PRIMARY KEY,
    aggregate_id     UUID             NOT NULL,
    sequence_number  INT              NOT NULL,
    event_type       VARCHAR(100)     NOT NULL,
    payload          JSONB            NOT NULL,
    user_id          VARCHAR(100)     NOT NULL,
    timestamp        TIMESTAMPTZ      NOT NULL,     -- Siempre UTC, sincronizado NTP
    previous_hash    CHAR(64)         NOT NULL,     -- SHA-256 del evento anterior
    hash             CHAR(64)         NOT NULL,     -- SHA-256 de este registro

    CONSTRAINT uq_aggregate_sequence UNIQUE (aggregate_id, sequence_number)
);

-- El usuario de aplicación SOLO tiene INSERT y SELECT
REVOKE DELETE, UPDATE ON event_store FROM app_user;
```

> **Nota de seguridad:** La restricción de permisos se aplica a nivel de base de datos, **independientemente** del código de la aplicación. Incluso si una vulnerabilidad en el API permitiera ejecutar comandos arbitrarios, `app_user` no puede borrar ni modificar registros.

---

## Cálculo del Hash (SHA-256 encadenado)

El `hash` de cada fila se calcula sobre la concatenación de sus campos inmutables más el `previous_hash` de la fila anterior. Esto forma una cadena donde cualquier modificación retrospectiva rompe el hash de todos los registros posteriores.

```
hash = SHA256(
    aggregate_id ||
    sequence_number ||
    event_type ||
    payload ||
    user_id ||
    timestamp ||
    previous_hash
)
```

El primer evento de la cadena global usa `previous_hash = "0000...0000"` (64 ceros).

---

## Tipos de evento (`event_type`)

| EventType               | Disparador                      | Campos clave en `payload`                            |
| ----------------------- | ------------------------------- | ---------------------------------------------------- |
| `ShipmentCreated`       | Operador crea un nuevo envío    | `product_id`, `origin`, `destination`, `target_temp` |
| `TemperatureRecorded`   | Telemetría IoT o backfill       | `value`, `unit`, `gps`, `battery_level`, `source`    |
| `ExcursionDetected`     | Motor de excursiones (< 10s)    | `value`, `duration_minutes`, `threshold`             |
| `ExcursionAcknowledged` | Inspector QA confirma excursión | `reason_code`, `payload_hash`                        |
| `ReleaseSigned`         | Firma electrónica QA (CFR 11)   | `payload_hash`, `reason_code`                        |
| `ShipmentRejected`      | Inspector QA rechaza lote       | `reason_code`, `payload_hash`                        |

---

## Ejemplo de payload — `TemperatureRecorded`

```json
{
  "event_type": "TemperatureRecorded",
  "aggregate_id": "3f2504e0-4f89-11d3-9a0c-0305e82c3301",
  "sensor_id": "sensor-mx-09",
  "value": -72.5,
  "unit": "°C",
  "gps": {
    "lat": 25.6866,
    "lng": -100.3161
  },
  "battery_level": 87,
  "source": "live",
  "comment": null
}
```

Para datos en backfill, el campo `source` toma el valor `"backfilled"` y el `timestamp` refleja la **hora original del sensor**, no la hora de recepción:

```json
{
  "event_type": "TemperatureRecorded",
  "aggregate_id": "3f2504e0-4f89-11d3-9a0c-0305e82c3301",
  "sensor_id": "sensor-mx-09",
  "value": -71.8,
  "unit": "°C",
  "gps": { "lat": 25.6901, "lng": -100.3200 },
  "battery_level": 52,
  "source": "backfilled",
  "received_at": "2026-04-28T14:35:00Z"
}
```

> `timestamp` (columna de la tabla) = hora original del sensor  
> `received_at` (campo del payload) = hora real de recepción por el API

---

## Ejemplo de payload — `ReleaseSigned` (CFR 11)

```json
{
  "event_type": "ReleaseSigned",
  "aggregate_id": "3f2504e0-4f89-11d3-9a0c-0305e82c3301",
  "payload_hash": "a3f1d2e4b5c6...",
  "reason_code": "ROUTINE_RELEASE",
  "signing_method": "password_reauth"
}
```

---

## Convenciones

| Convención                   | Regla                                                                                                   |
| ---------------------------- | ------------------------------------------------------------------------------------------------------- |
| **Timestamps**               | Siempre `TIMESTAMPTZ` en UTC. Todos los servicios sincronizados via NTP.                                |
| **IDs**                      | UUID v4 generado en el dominio, no en la base de datos.                                                 |
| **Nombres de tabla/columna** | `snake_case` (convención PostgreSQL).                                                                   |
| **Sin DELETE**               | La tabla no soporta borrado físico. Toda anulación es un nuevo evento (`ShipmentRejected`, etc.).       |
| **Payload**                  | `JSONB` permite indexación y consultas sobre campos específicos del payload sin deserializar en el API. |
