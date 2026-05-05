# ADR-09: Estrategia de Retención de Datos (Hot / Warm / Cold)

**Estado:** Aceptado  
**Fecha:** 2026-05-01  
**Autores:** Equipo CryoLogistics

---

## Contexto

La normativa GxP y las guías FDA para terapias génicas requieren retener registros electrónicos por **al menos el tiempo de vida del producto + 2 años** — en la práctica, **10-15 años**. Mantener toda la historia en PostgreSQL indefinidamente no es viable:

- 100k sensores × 1 evento/60s = ~1,667 eventos/segundo
- ~144M filas/día × ~500 bytes/fila promedio = **~70GB/día** solo en el Event Store
- A 12 meses: ~25TB en PostgreSQL

Se requiere una estrategia de archivado que preserve la integridad del hash chain y cumpla con los requerimientos regulatorios de accesibilidad de registros.

---

## Decisión

Se implementa una estrategia de tres tiers con archivado progresivo basado en el ciclo de vida del envío.

| Tier     | Almacenamiento                 | Datos                                     | Criterio de permanencia       | Latencia de acceso            |
| -------- | ------------------------------ | ----------------------------------------- | ----------------------------- | ----------------------------- |
| **Hot**  | Redis (Read Store)             | Estado actual de envíos activos           | `status = active`             | < 1ms                         |
| **Warm** | PostgreSQL (Event Store)       | Todos los eventos de los últimos 12 meses | Envío cerrado hace < 12 meses | < 50ms                        |
| **Cold** | Object Storage (S3-compatible) | Eventos archivados en JSON comprimido     | Envío cerrado hace ≥ 12 meses | Minutos (restauración manual) |

---

## Justificación

### Período de retención Warm (12 meses)

La mayoría de las auditorías e investigaciones de desviación ocurren dentro del año siguiente al cierre del envío. 12 meses en PostgreSQL cubre el 95%+ de los casos de acceso frecuente. Los accesos post-12 meses son excepcionales (inspecciones regulatorias, litigios) y toleran restauración manual desde Cold.

A 12 meses de retención Warm, el Event Store mantiene un volumen manejable de ~25TB — escalable con particionado por rango de fecha sobre `timestamp`.

### Integridad del hash chain durante el archivado

El archivado **no viola la inmutabilidad del hash chain**. El proceso es:

```
1. Seleccionar eventos del agregado a archivar (shipment cerrado > 12 meses)
2. Exportar bloque de eventos como JSON con GZIP
3. Verificar hash chain del bloque exportado (primer y último hash)
4. Registrar evento de auditoría: ArchiveCompleted { aggregate_id, first_hash, last_hash, cold_path }
5. Eliminar filas del Warm tier (PostgreSQL)
```

Los eventos nunca se modifican — se copian íntegramente y luego se eliminan del Warm. La cadena de hashes es verificable desde el Cold tier. El evento `ArchiveCompleted` en el Event Store actúa como prueba de custodia: un inspector FDA puede verificar que el bloque archivado corresponde a los hashes registrados en el momento del archivado.

### Object Storage agnóstico de proveedor

Para mantener independencia de infraestructura (consistente con ADR-05), el Cold tier usa la **API S3-compatible** — soportada por AWS S3, Azure Blob Storage (via compatibilidad S3), Google Cloud Storage y **MinIO** (self-hosted). El cliente de archivado usa el SDK de S3 estándar sin lock-in a un proveedor específico.

La **encriptación at-rest** (NFR-4) se habilita a nivel del bucket/contenedor — nativa en todos los proveedores mencionados, sin configuración adicional en el código de la aplicación.

### Consulta forense en Cold tier

El acceso al Cold tier es un proceso manual documentado:
1. Identificar el `aggregate_id` del envío via el Event Store (el evento `ArchiveCompleted` contiene el `cold_path`).
2. Descargar el bloque JSON comprimido del Cold tier.
3. Verificar hash chain del bloque restaurado contra los hashes registrados en `ArchiveCompleted`.
4. Importar al entorno de análisis para inspección.

Las auditorías FDA no son en tiempo real — un proceso de restauración de minutos es aceptable para este caso de uso.

---

## Consecuencias

**Positivas:**
- PostgreSQL se mantiene en un tamaño manejable con particionado por rango de fecha.
- La integridad del hash chain se preserva y es verificable en el Cold tier.
- El Cold tier es agnóstico de proveedor — sin lock-in.
- La encriptación at-rest cumple NFR-4 sin código adicional.

**Negativas:**
- El proceso de archivado agrega un componente operacional (job periódico, ej. mensual) que debe ser monitoreado.
- El acceso forense al Cold tier requiere un proceso manual documentado — no hay consulta online directa.
- La eliminación de filas del Warm tier, aunque auditada, requiere un usuario de base de datos con permisos `DELETE` sobre filas archivadas — excepción controlada a la restricción general de `REVOKE DELETE ON event_store FROM app_user`.

**Mitigación de la excepción DELETE:** El job de archivado usa un usuario de base de datos dedicado (`archiver_user`) con permisos `DELETE` únicamente sobre filas donde el `aggregate_id` está en estado `archived` (validado por el evento `ArchiveCompleted`). El `app_user` de la aplicación mantiene solo `INSERT` y `SELECT`.


---

## Alternativas descartadas

- **Retención ilimitada en PostgreSQL:** ~25TB/año en el Event Store es insostenible operacionalmente y económicamente para un sistema de 10-15 años. Descartado.
- **Archivado sin eliminación del Warm tier (solo copia):** Resuelve la integridad pero no el problema de crecimiento de PostgreSQL. Descartado.
- **Consulta forense online (Athena, BigQuery, ClickHouse sobre Cold):** Agrega un cuarto componente de infraestructura para un caso de uso excepcional. Las auditorías FDA toleran restauración manual. Descartado para el alcance actual.
