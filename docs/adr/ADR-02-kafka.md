# ADR-02: Apache Kafka como bus de eventos

**Estado:** Aceptado  
**Fecha:** 2026-04-28  
**Autores:** Equipo CryoLogistics

---

## Contexto

El sistema necesita un bus de mensajería para desacoplar el Event Store del Projector y del Excursion Engine. Los requerimientos críticos que condicionan esta decisión son:

- **FR-5 — Backfilling:** Cuando un sensor recupera conectividad, envía datos históricos con timestamps originales. El sistema debe poder "rebobinar" y reprocessar eventos desde un punto en el tiempo sin perder mensajes.
- **NFR-5 — 100,000 sensores concurrentes:** 100k sensores × 1 evento/60s = ~1,667 eventos/segundo en estado estable, con picos potencialmente mayores.
- **NFR-2 — Disponibilidad 99.9%:** La pérdida de telemetría es una falla de cumplimiento crítica.

---

## Decisión

Se utiliza **Apache Kafka** como bus de eventos con un topic `shipment-events`.

---

## Justificación

| Criterio                               | Kafka                         | RabbitMQ                           | Redis Streams                    |
| -------------------------------------- | ----------------------------- | ---------------------------------- | -------------------------------- |
| **Backfilling nativo** (FR-5)          | ✅ `seek` por offset/timestamp | ❌ Mensajes consumidos = eliminados | ⚠️ Solo si no expiró la retención |
| **Throughput sostenido**               | ✅ Millones/seg                | ⚠️ ~50k/seg con clustering          | ✅ ~1M/seg                        |
| **Retención configurable**             | ✅ Días/semanas/ilimitada      | ❌ No — TTL solo                    | ⚠️ Limitada por RAM               |
| **Múltiples consumers independientes** | ✅ Consumer groups nativo      | ⚠️ Requiere exchanges/bindings      | ✅ Consumer groups                |
| **Replay de eventos**                  | ✅ Fundamental al modelo       | ❌ No soporta                       | ⚠️ Limitado                       |
| **Complejidad operacional**            | ⚠️ Media-alta (KRaft)          | ✅ Baja                             | ✅ Muy baja                       |

El factor determinante es **FR-5 (Backfilling)**. Con RabbitMQ, implementar backfilling requiere releer desde el Event Store SQL y reprocessar manualmente, agregando complejidad significativa al Projector. Con Kafka, el Projector simplemente hace `seek` al offset del timestamp donde el sensor perdió conexión — es nativo al modelo de log de Kafka.

Los dos consumer groups independientes (`projector` y `excursion`) consumen el mismo topic sin interferencia, lo que permite escalar cada Worker Service de forma independiente.

---

## Consecuencias

**Positivas:**
- Backfilling implementable con `seek` nativo — sin lógica adicional en el Projector.
- Retención configurable permite replay completo del Event Store para reconstruir proyecciones.
- Los dos consumer groups (Projector + Excursion Engine) operan completamente desacoplados.
- Throughput más que suficiente para 100k sensores con margen amplio.

**Negativas:**
- Mayor complejidad operacional que RabbitMQ o Redis Streams.
- Requiere configuración de KRaft (o ZooKeeper en versiones antiguas) en el entorno Docker.
- La curva de aprendizaje de Kafka es más pronunciada.

**Mitigación:** El entorno de desarrollo usa `confluentinc/cp-kafka` en modo KRaft single-node en Docker Compose. Para producción se considera Confluent Cloud o MSK (Amazon Managed Streaming for Kafka).

---

## Alternativas descartadas

- **RabbitMQ:** Excelente para work queues y routing complejo. No soporta replay de mensajes — incompatible con FR-5 sin diseño adicional significativo. Descartado.
- **Redis Streams:** Ya presente en el stack como Read Store. Evitaría un componente extra. Sin embargo, la retención está limitada por RAM y el replay por tiempo de retención configurable, no por offset infinito. Insuficiente para garantizar FR-5 en escenarios de larga desconexión. Descartado.
