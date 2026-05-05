# ADR-04: Keycloak como Identity Provider

**Estado:** Aceptado  
**Fecha:** 2026-04-28  
**Autores:** Equipo CryoLogistics

---

## Contexto

El sistema requiere un Identity Provider (IdP) para dos responsabilidades distintas:

1. **Autenticación general:** Login, sesión, RBAC con roles `Operator`, `QA`, `Admin`.
2. **Signing Challenge (CFR 11):** Re-autenticación explícita al momento de la firma electrónica, sin destruir la sesión activa del usuario. Requiere validar credenciales on-demand via API.

Las opciones evaluadas son:

- **Keycloak** — IdP open-source, self-hosted, con soporte OAuth2/OIDC/SAML.
- **Azure Active Directory (Azure AD / Entra ID)** — IdP gestionado de Microsoft.
- **IdentityServer / Duende IdentityServer** — librería .NET para construir un IdP propio.

---

## Decisión

Se utiliza **Keycloak** como Identity Provider.

---

## Justificación

| Criterio                                              | Keycloak                         | Azure AD                        | IdentityServer (Duende)       |
| ----------------------------------------------------- | -------------------------------- | ------------------------------- | ----------------------------- |
| **Self-hosted / Docker**                              | ✅ Imagen oficial, Docker Compose | ❌ SaaS, dependencia de nube     | ✅ Librería .NET               |
| **Costo**                                             | ✅ Gratuito (open-source)         | ⚠️ Costo por tenant/usuario      | ⚠️ Licencia comercial (Duende) |
| **Signing Challenge** (Resource Owner Password Grant) | ✅ Soportado explícitamente       | ⚠️ Depreciado en flujos modernos | ✅ Configurable                |
| **RBAC con roles custom**                             | ✅ Roles de realm y de cliente    | ✅ App Roles                     | ✅ Claims custom               |
| **Sin dependencia de nube**                           | ✅ Completamente autocontenido    | ❌ Requiere Azure subscription   | ✅                             |
| **Admin UI para gestión de usuarios**                 | ✅ Incluida                       | ✅ Azure Portal                  | ❌ No incluida                 |
| **Entorno One-Click (Docker Compose)**                | ✅ `keycloak/keycloak:latest`     | ❌ No dockerizable               | ✅ Parte del API               |

El requerimiento más crítico es el **Signing Challenge**: la re-autenticación explícita con contraseña al momento de firmar un lote (CFR 11 § 11.200). Esto requiere el *Resource Owner Password Grant* (ROPC) de OAuth2, donde el API valida directamente usuario + contraseña contra el IdP en nombre del usuario sin redirigirlo. Keycloak soporta ROPC de forma explícita y configurable.

Azure AD ha marcado ROPC como depreciado en favor de flujos interactivos — su soporte futuro es incierto. IdentityServer requeriría construir la UI de administración desde cero.

La prioridad de que el entorno sea **100% autocontenido con Docker Compose** (sin dependencias de nube, sin cuentas externas).

---

## Consecuencias

**Positivas:**
- Entorno de desarrollo 100% autocontenido — `docker compose up` levanta todo incluyendo el IdP.
- ROPC disponible para el Signing Challenge sin workarounds.
- Consola de administración incluida para gestionar usuarios, roles y realms.
- Exportación/importación de realm configuration como JSON — reproducible en CI/CD.

**Negativas:**
- Keycloak consume ~512MB RAM en modo standalone — relevante en entornos con recursos limitados.
- La configuración inicial del realm y los clients requiere un paso de setup (exportar config como JSON y comitearla).
- En producción, Keycloak requiere su propia base de datos (PostgreSQL recomendado) y alta disponibilidad.

**Mitigación:** El realm de desarrollo se exporta como `keycloak-realm.json` y se importa automáticamente al levantar el contenedor. En producción, Keycloak se despliega con PostgreSQL dedicado en modo clustering.

---

## Alternativas descartadas

- **Azure AD:** ROPC depreciado, dependencia de nube, costo por usuario. Incompatible con el requerimiento de entorno autocontenido. Descartado.
- **IdentityServer / Duende:** Requiere implementar desde cero la UI de administración de usuarios. La licencia comercial de Duende agrega costo. No aporta ventajas sobre Keycloak para este caso de uso. Descartado.
