# ADR-07: Estrategia de Snapshotting del Agregado

**Estado:** Aceptado  
**Fecha:** 2026-05-01  
**Autores:** Equipo CryoLogistics

---

## Contexto

El modelo de Event Sourcing requiere reconstruir el estado actual de un agregado (`Shipment`) leyendo todos sus eventos desde el origen. Para envíos de larga duración, esto podría implicar cargar miles de eventos `TemperatureRecorded` para obtener el estado actual.

El Snapshotting resuelve esto guardando periódicamente una "foto" del estado en un punto dado, evitando releer eventos históricos completos en cada reconstrucción.

---

## Decisión

**No se implementa snapshotting.** El write side usa carga selectiva de eventos; el read side usa Redis como estado materializado.
 
---

## Justificación

### Volumen de eventos por agregado

Un envío de terapia génica dura entre 1 y 14 días con 1 evento `TemperatureRecorded` cada 60 segundos:

| Duración del envío | Eventos `TemperatureRecorded` |
| ------------------ | ----------------------------- |
| 1 día              | ~1,440                        |
| 7 días             | ~10,080                       |
| 14 días            | ~20,160                       |

PostgreSQL con el índice `(aggregate_id, sequence_number)` lee 20k filas en el orden de decenas de milisegundos. El volumen por sí solo no justifica snapshotting.

### El write side no reconstruye estado completo

El command handler nunca necesita reproducir todos los eventos para ejecutar un comando. Solo requiere:

| Información necesaria                             | Query en el Event Store                                                                 |
| ------------------------------------------------- | --------------------------------------------------------------------------------------- |
| `sequence_number` actual (optimistic concurrency) | `SELECT MAX(sequence_number) WHERE aggregate_id = ?` — O(1)                             |
| `user_id` del creador (validación Maker-Checker)  | `SELECT payload WHERE aggregate_id = ? AND event_type = 'ShipmentCreated'` — O(1)       |
| Estado actual del envío (activo / cerrado)        | `SELECT event_type WHERE aggregate_id = ? ORDER BY sequence_number DESC LIMIT 1` — O(1) |

Los tres accesos son queries puntuales indexadas. No se requiere cargar ni deserializar la cadena completa de eventos para ningún comando del sistema.

### El read side ya tiene estado materializado

El Projector mantiene el estado actual de cada `Shipment` en Redis (`HSET`). Todas las consultas del dashboard y los workers leen desde Redis — nunca reconstruyen el agregado desde el Event Store. El Read Store en Redis cumple funcionalmente el mismo rol que un snapshot para el lado lectura.

### Comparativa de estrategias

| Estrategia                             | Complejidad                         | Aplica en este sistema      |
| -------------------------------------- | ----------------------------------- | --------------------------- |
| **Sin snapshotting + carga selectiva** | Mínima                              | ✅ Seleccionada              |
| Snapshot cada N eventos                | Media (tabla adicional, versionado) | ❌ No necesario              |
| Snapshot al cerrar envío               | Baja-media                          | ❌ Redis ya cubre este caso  |
| Redis como snapshot del read side      | Ya implementado                     | ✅ Cubierto por el Projector |

--- 

## Consecuencias

**Positivas:**
- Sin tabla adicional de snapshots ni lógica de versionado de estado serializado.
- El Event Store permanece como única fuente de escritura — sin estructuras de soporte adicionales.
- Los tres queries de validación del write side son O(1) sobre índices existentes.

**Negativas:**
- Si la duración típica de envíos aumentara significativamente (meses), o si los comandos requirieran acceso a más estado histórico, esta decisión debería revisarse.

**Condición de revisión:** Si la latencia del write side supera 100ms de forma sostenida en producción, evaluar snapshot cada N=1,000 eventos como primera mitigación.

---

## Alternativas descartadas

- **Snapshot cada N eventos:** Requiere tabla adicional, lógica de serialización del estado del agregado, y manejo de versiones de schema del snapshot. El beneficio no justifica la complejidad dado que el write side no reconstruye estado completo.
- **Snapshot al cerrar envío:** Elegante, pero Redis ya materializa el estado final del envío cuando se emite `ReleaseSigned`. Duplicar esa información en el Event Store no aporta valor.
