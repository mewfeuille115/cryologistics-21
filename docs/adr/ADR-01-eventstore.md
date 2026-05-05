# ADR-01: Event Store propio sobre PostgreSQL vs librería existente

**Estado:** Aceptado  
**Fecha:** 2026-04-28  
**Autores:** Equipo CryoLogistics

---

## Contexto

El sistema requiere un Audit Trail inmutable con hashing SHA-256 encadenado por requerimiento regulatorio (FDA 21 CFR Part 11 / NFR-1 / ALCOA+). Se evaluó si construir el Event Store desde cero sobre una base de datos relacional o adoptar una solución existente.

Las opciones evaluadas fueron:

- **Marten** — librería .NET que convierte PostgreSQL en un Event Store y Document Database.
- **EventStoreDB** — base de datos diseñada específicamente para Event Sourcing (Greg Young).
- **Implementación propia sobre PostgreSQL** — tabla `event_store` con lógica de hashing en la capa de aplicación.

---

## Decisión

Se implementa un **Event Store propio sobre PostgreSQL**.

---

## Justificación

| Criterio                                     | Marten                                       | EventStoreDB            | Propio sobre PostgreSQL |
| -------------------------------------------- | -------------------------------------------- | ----------------------- | ----------------------- |
| **Hash encadenado SHA-256**                  | ❌ No incluido                                | ❌ No incluido           | ✅ Control total         |
| **Permisos DB granulares** (`REVOKE DELETE`) | ⚠️ Posible, pero la librería puede interferir | ❌ No aplica — no es SQL | ✅ Nativo PostgreSQL     |
| **Curva de aprendizaje**                     | Media                                        | Alta                    | Baja                    |
| **Dependencia adicional**                    | Sí                                           | Sí (nuevo motor)        | No                      |
| **Auditabilidad del código**                 | ⚠️ Caja gris                                  | ❌ Caja negra            | ✅ Caja blanca completa  |

El requerimiento de hash SHA-256 encadenado no es una característica estándar de ninguna librería de Event Sourcing. Implementarlo como una capa sobre Marten o EventStoreDB requeriría interceptar el proceso de escritura de formas no documentadas, creando una dependencia frágil.

Con una implementación propia, el encadenamiento de hashes es explícito, auditado, testeable de forma aislada, y comprobable por un auditor de cumplimiento sin conocimiento de librerías externas.

---

## Consecuencias

**Positivas:**
- Control total sobre el esquema, los permisos de base de datos y la lógica de hashing.
- Código auditable line-by-line para inspectores de cumplimiento.
- Sin dependencia de librerías que podrían cambiar su API o comportamiento interno.

**Negativas:**
- Se implementa manualmente lo que Marten/EventStoreDB proveen (append, load by aggregate, optimistic concurrency).
- Mayor superficie de código a mantener y testear.

**Mitigación:** Las operaciones del Event Store son un conjunto acotado y estable (`AppendEvents`, `LoadEvents`, `VerifyHashChain`). Los tests de integridad del hash chain se incluyen en el pipeline CI/CD.

---

## Alternativas descartadas

- **Marten:** Excelente librería, pero no soporta hash chaining nativo. Requeriría hooks de escritura no documentados para cumplir NFR-1. Descartado.
- **EventStoreDB:** Motor externo adicional (KRaft-like, propio). Agrega complejidad operacional y no soporta `REVOKE DELETE` a nivel SQL, haciendo imposible la garantía de inmutabilidad a nivel de DB. Descartado.
