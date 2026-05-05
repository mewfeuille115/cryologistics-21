# ADR-08: Garantía de Consistencia Eventual Write/Read

**Estado:** Aceptado  
**Fecha:** 2026-05-01  
**Autores:** Equipo CryoLogistics

--- 

## Contexto

El sistema usa CQRS con bases de datos separadas: el Write Store (PostgreSQL) es la fuente de verdad, y el Read Store (Redis) es una proyección eventual. La secuencia desde que un comando es aceptado hasta que el cliente ve el nuevo estado es:

```
API persiste evento en PostgreSQL  (~1-5ms)
        ↓
API publica mensaje en Kafka        (~1-5ms)
        ↓
Projector consume mensaje           (~5-50ms, depende del poll interval)
        ↓
Projector escribe Redis + PUBLISH   (~1-5ms)
        ↓
API pushea via SSE al cliente       (~1-2ms)
```

Hay una ventana de inconsistencia entre el `200 OK` del comando y la actualización visible en el dashboard.

---

## Decisión

Se **acepta la consistencia eventual** sin mecanismos adicionales de sincronización. El pipeline existente (Redis Pub/Sub → SSE) entrega la actualización al cliente dentro del budget de NFR-3. El frontend maneja la ventana con un indicador de estado transitorio.

---

## Justificación

### Latencia del pipeline

| Segmento                       | Latencia estimada P50 | Latencia estimada P99 |
| ------------------------------ | --------------------- | --------------------- |
| PostgreSQL INSERT              | ~1-5ms                | ~10ms                 |
| Kafka produce → consumer poll  | ~5-20ms               | ~50-100ms             |
| Projector procesa + Redis HSET | ~1-5ms                | ~10ms                 |
| Redis PUBLISH → SSE push       | ~1-2ms                | ~5ms                  |
| **Total pipeline**             | **~10-30ms**          | **~50-150ms**         |

El P99 estimado de ~150ms está dentro del budget de **NFR-3 (< 200ms desde la ingesta)**. NFR-3 cubre el pipeline completo — desde que el API acepta el dato hasta que el SSE llega al cliente.

### El SSE elimina la necesidad de polling

El cliente ya tiene una conexión SSE activa sobre el envío que está monitoreando. No necesita hacer `GET` para verificar si el estado cambió — el servidor empuja la actualización cuando el Projector completa. La ventana de inconsistencia no requiere que el usuario recargue ni consulte activamente.

### Comparativa de estrategias

| Estrategia                                      | Complejidad                                     | Aplica en este sistema                  |
| ----------------------------------------------- | ----------------------------------------------- | --------------------------------------- |
| **Aceptar eventual consistency + UX guiada**    | Mínima                                          | ✅ Seleccionada                          |
| Read-your-writes via `X-Sequence-Number` header | Alta — polling activo en el endpoint de lectura | ❌ Innecesario dado el P99 < 200ms       |
| Optimistic UI update                            | Media — riesgo de estado divergente             | ❌ Riesgo inaceptable en contexto CFR 11 |

### Caso crítico: firma electrónica QA

Cuando el Inspector de QA firma un lote (`POST /api/shipments/{id}/sign-release`):

1. El API retorna `200 OK` con el `sequence_number` del evento `ReleaseSigned`.
2. El frontend muestra el estado como **"Firma procesándose..."** (estado transitorio).
3. El SSE entrega la actualización en < 200ms — el dashboard muestra **"Lote liberado"** automáticamente.

El estado transitorio acota la ventana de ambigüedad para el usuario. No hay posibilidad de interpretación incorrecta: el comando fue aceptado (registrado en el Audit Trail), y la confirmación visual llega en milisegundos.

La estrategia de Optimistic UI Update se descarta explícitamente para acciones de firma: en un contexto CFR 11, mostrar un estado que aún no está confirmado por el Event Store introduce riesgo de discrepancia entre lo que el usuario vio y lo que el Audit Trail registró.

---

## Consecuencias

**Positivas:**
- Sin complejidad adicional en el API ni en el frontend más allá del indicador de estado transitorio.
- El SSE ya es el canal de notificación — no hay polling, no hay headers especiales.
- Compatible con el modelo de Audit Trail: el `200 OK` significa "registrado", no "visible en el dashboard".

**Negativas:**
- Si el Projector se retrasa significativamente (lag de Kafka elevado), el estado transitorio podría durar más de lo esperado. Requiere monitoreo del consumer lag.

**Condición de revisión:** Si el P99 del pipeline supera 500ms de forma sostenida en producción, evaluar read-your-writes via `sequence_number` como mecanismo de fallback para acciones críticas de firma.

---

## Alternativas descartadas

- **Read-your-writes via `X-Sequence-Number`:** El endpoint de lectura esperaría hasta que Redis tenga el `sequence_number` indicado antes de responder. Agrega complejidad significativa (polling interno, timeouts, riesgo de bloqueo) para una ventana de inconsistencia que el SSE resuelve automáticamente.
- **Optimistic UI update:** El frontend actualizaría el estado localmente tras el `200 OK` sin esperar el SSE. Descartado para acciones de firma: en contexto CFR 11, el estado visible debe reflejar lo que el Audit Trail confirma, no una proyección optimista del cliente.
