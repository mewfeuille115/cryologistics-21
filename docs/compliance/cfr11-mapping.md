# Cumplimiento FDA 21 CFR Part 11

> [← README](../../README.md)

Esta sección define cómo cada requerimiento de la norma se traduce en una decisión técnica concreta del sistema.

---

## 4.1 Audit Trail Inmutable (§ 11.10(e))

**Requerimiento normativo:** Los sistemas computarizados deben mantener registros de auditoría que incluyan la fecha y hora de creación y modificación de registros electrónicos, y la identidad del operador.

**Implementación técnica:**

- Cada fila del Event Store incluye: `user_id`, `timestamp` (UTC, sincronizado NTP), `event_type`, `payload` (JSON con `old_value` / `new_value` donde aplica), y `reason`.
- El API **no expone** endpoints `DELETE` ni `UPDATE` sobre los eventos de auditoría.
- El usuario de base de datos de la aplicación (`app_user`) tiene exclusivamente permisos de `INSERT` y `SELECT` sobre la tabla `event_store`:

```sql
REVOKE DELETE, UPDATE ON event_store FROM app_user;
```

- El encadenamiento de hashes SHA-256 (`previous_hash` → `hash`) permite detectar manipulación directa a nivel de base de datos, incluso si un actor con acceso DBA modifica filas sin pasar por el API (NFR-1 / ALCOA+).

---

## 4.2 Firma Electrónica y No Repudio (§ 11.50 / § 11.70)

**Requerimiento normativo:** Las firmas electrónicas deben ser únicas, vinculadas permanentemente al registro firmado, y el firmante debe indicar el significado de la firma (razón/motivo).

**Implementación técnica — Signing Challenge:**

El flujo no destruye la sesión activa del usuario. El modal de firma requiere re-autenticación explícita:

1. El Inspector de QA hace clic en **"Firmar y Liberar"**.
2. El frontend muestra un modal que solicita: **Contraseña** + **ReasonCode** (campo obligatorio, no puede estar vacío).
3. El backend ejecuta un *Resource Owner Password Grant* contra **Keycloak** para validar las credenciales.
4. Si son válidas, se inserta el evento `ReleaseSigned` que incluye:

| Campo          | Contenido                                           | Propósito normativo                    |
| -------------- | --------------------------------------------------- | -------------------------------------- |
| `user_id`      | Identificador único del firmante                    | Atribución (ALCOA+)                    |
| `timestamp`    | UTC sincronizado por NTP                            | Contemporaneidad (ALCOA+)              |
| `payload_hash` | SHA-256 del estado completo del lote en ese momento | Vinculación firma ↔ registro (§ 11.70) |
| `reason_code`  | Motivo explícito seleccionado por el usuario        | Significado de la firma (§ 11.50)      |

> Un intento fallido de Signing Challenge retorna `401 Unauthorized` y **no inserta ningún evento**. El intento se registra en los logs de auditoría del API.

---

## 4.3 Manejo de Concurrencia — Principio de Cuatro Ojos

**Requerimiento normativo:** El sistema debe prevenir que una misma persona cree y libere el mismo lote (Maker-Checker).

**Implementación técnica — Optimistic Concurrency:**

Cada agregado (`Shipment`) mantiene un `sequence_number` que se incrementa con cada evento. Al ejecutar un comando, el cliente envía la versión que tiene:

- Si el `sequence_number` del cliente coincide con el del Event Store → la operación procede y se incrementa a `n+1`.
- Si no coincide → `409 Conflict: El estado del lote ha cambiado. Por favor, recarga la información.`

Este mecanismo previene que dos Inspectores de QA firmen decisiones contradictorias sobre el mismo lote simultáneamente, sin requerir locks a nivel de base de datos.

La restricción Maker-Checker se valida en el dominio: el `user_id` del evento `ShipmentCreated` no puede ser igual al `user_id` del evento `ReleaseSigned` sobre el mismo agregado.

---

## Matriz de Cumplimiento

| Artículo CFR 11 | Requerimiento                    | Mecanismo de cumplimiento                                           |
| --------------- | -------------------------------- | ------------------------------------------------------------------- |
| § 11.10(a)      | Validación de sistemas           | Tests de integridad del hash chain en CI/CD                         |
| § 11.10(b)      | Capacidad de generar copias      | Endpoint de exportación de eventos (Fase 2)                         |
| § 11.10(d)      | Acceso limitado                  | RBAC con roles `Operator`, `QA`, `Admin` via Keycloak               |
| § 11.10(e)      | Audit Trail                      | Event Store inmutable con hash SHA-256 encadenado                   |
| § 11.10(f)      | Secuencia operativa              | `SequenceNumber` por agregado + Optimistic Concurrency              |
| § 11.10(g)      | Autorización y acceso            | OAuth2/OIDC + RBAC — Keycloak como Identity Provider                |
| § 11.50         | Componentes de firma             | `UserId` + `Timestamp` + `ReasonCode` + `PayloadHash`               |
| § 11.70         | Vinculación firma-registro       | `payload_hash` = SHA-256 del estado del lote al momento de la firma |
| § 11.100        | Unicidad                         | `user_id` único por registro + hash encadenado                      |
| § 11.200        | Componentes de firma electrónica | Re-autenticación explícita (contraseña) en cada firma               |
