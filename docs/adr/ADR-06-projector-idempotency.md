# ADR-06: Estrategia de Idempotencia del Projector

**Estado:** Aceptado  
**Fecha:** 2026-05-01  
**Autores:** Equipo CryoLogistics

---

## Contexto

El Projector consume eventos de Kafka y actualiza proyecciones en Redis. En escenarios de falla (crash del contenedor, rebalanceo de particiones), Kafka puede reentrar eventos ya procesados. Procesar el mismo evento dos veces puede resultar en estado corrupto en Redis (contadores duplicados, temperatura sobrescrita con valor viejo, etc.).

Se necesita una estrategia para garantizar que cada evento se procese **exactamente una vez** (o que el procesamiento sea idempotente si ocurre más de una vez).

---

## Opciones evaluadas

| Estrategia                               | Mecanismo                                                                         | Trade-offs                                                                      | Decisión      |
| ---------------------------------------- | --------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- | ------------- |
| **Idempotency Key en Redis**             | `SET processed:{event_id} 1 EX 86400` antes de procesar                           | Simple de implementar. Introduce una RTT extra por evento. TTL puede expirar.   | ❌ Descartado  |
| **Transactional Outbox + exactly-once**  | Kafka Transactions + `enable.idempotence=true`                                    | Exactly-once real, pero requiere coordinación transaccional Kafka ↔ Redis.      | ❌ Descartado  |
| **Proyecciones idempotentes por diseño** | `HSET` con guardia: actualizar solo si `sequence_number > stored_sequence_number` | Natural al modelo de Event Sourcing. Sin operación extra en el caso base.       | ✅ **Elegido** |
| **Offset commit manual post-proceso**    | `enable.auto.commit=false`. Commit tras éxito en Redis.                           | Reduce la ventana de duplicados pero no los elimina. Complemento, no sustituto. | ⚠️ Complemento |

---

## Preguntas abiertas — resueltas

- [x] **¿Es suficiente la estrategia de proyecciones idempotentes por `sequence_number`?**
  Sí. El Event Store garantiza que `sequence_number` es estrictamente creciente por agregado (control de concurrencia optimista — ADR-01). Un evento duplicado siempre llegará con un `sequence_number` igual o menor al ya almacenado en Redis; la guardia `sequence > stored_sequence` lo descarta sin efecto secundario.

- [x] **¿Cuál es el RTO aceptable si el Projector falla y pierde su offset?**
  El Projector no mantiene estado propio de offset; Kafka lo persiste en el consumer group. Ante un crash, el consumer group retoma desde el último offset committeado. El offset se commitea manualmente (`enable.auto.commit=false`) solo tras escritura exitosa en Redis, combinando así ambas estrategias. El RTO es el tiempo de restart del contenedor — aceptable bajo el diseño de alta disponibilidad (NFR-2).

- [x] **¿Se requiere Dead Letter Queue (DLQ) para eventos que fallan repetidamente?**
  Sí, como complemento operacional. Si un evento falla N veces consecutivas (e.g., 3 reintentos), se envía a un topic `shipment-events.dlq` para inspección manual sin bloquear el consumer group. Esto no afecta la estrategia de idempotencia; es una práctica de resiliencia estándar en Kafka.

---

## Decisión

Se adoptan **proyecciones idempotentes por diseño** usando `sequence_number` como guardia, complementado con **offset commit manual post-proceso** y un **Dead Letter Queue** para fallos persistentes.

### Mecanismo central

El Projector almacena en Redis, por cada agregado (`shipment_id`), el `sequence_number` del último evento aplicado:

```
HSET shipment:{id} ... sequence_number {n}
```

Antes de aplicar cualquier evento, verifica:

```
si event.sequence_number <= stored_sequence_number → descartar (no-op)
si event.sequence_number > stored_sequence_number  → aplicar y actualizar
```

Esta lógica es segura porque el Event Store garantiza secuencias estrictamente crecientes por agregado (optimistic concurrency — ADR-01), y Redis opera en un único hilo por clave (`HSET` es atómico).

---

## Consecuencias

**Positivas:**
- Sin operación adicional en el caso base (cero overhead por evento bien formado).
- Coherente con el modelo de Event Sourcing ya establecido; `sequence_number` ya existe en el schema del Event Store (ADR-01).
- La guardia es determinista y no depende de TTLs ni de ventanas de tiempo.
- El offset manual elimina la ventana de duplicados en la mayoría de los escenarios de crash.

**Negativas:**
- El campo `sequence_number` del agregado debe almacenarse en Redis junto al resto de la proyección — campo adicional por agregado.
- En escenarios de rebalanceo de particiones con lag alto, pueden llegar eventos fuera de orden entre particiones distintas (aunque no dentro de la misma partición). La guardia por `sequence_number` los maneja correctamente.

**Neutras:**
- El DLQ requiere un proceso operacional de revisión periódica.
