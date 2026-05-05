# ADR-03: SSE (Server-Sent Events) vs WebSockets / SignalR

**Estado:** Aceptado  
**Fecha:** 2026-04-28  
**Autores:** Equipo CryoLogistics

---

## Contexto

El dashboard de QA necesita reflejar el estado actual de los envíos en **< 200ms** (NFR-3) desde que el dato es ingestado. Se requiere un mecanismo de push desde el servidor hacia los clientes conectados. Las opciones evaluadas son:

- **Polling REST** — el cliente consulta el estado periódicamente.
- **SSE (Server-Sent Events)** — el servidor mantiene una conexión HTTP abierta y empuja eventos unidireccionalmente.
- **WebSockets / SignalR** — protocolo bidireccional full-duplex sobre TCP.

---

## Decisión

Se utiliza **SSE (Server-Sent Events)** via el endpoint `GET /api/shipments/{id}/stream`.

---

## Justificación

El caso de uso del dashboard es **exclusivamente unidireccional**: el servidor empuja actualizaciones de temperatura, GPS y estado. El cliente nunca envía datos por ese canal — las acciones del usuario (firmar lote, confirmar alerta) van por peticiones REST convencionales.

| Criterio                          | Polling REST            | SSE                                 | WebSockets / SignalR     |
| --------------------------------- | ----------------------- | ----------------------------------- | ------------------------ |
| **Cumple NFR-3 (< 200ms)**        | ❌ Depende del intervalo | ✅ Sí                                | ✅ Sí                     |
| **Dirección de comunicación**     | Cliente → Servidor      | Servidor → Cliente (unidireccional) | Bidireccional            |
| **Adecuación al caso de uso**     | ❌ Excesivo overhead     | ✅ Exactamente lo necesario          | ⚠️ Sobredimensionado      |
| **Complejidad de implementación** | Mínima                  | Baja                                | Media                    |
| **Reconexión automática**         | Manual                  | ✅ Nativa en el browser              | Manual                   |
| **HTTP/2 multiplexing**           | ✅                       | ✅                                   | ❌ Propio protocolo       |
| **Soporte de proxies/firewalls**  | ✅ Transparente          | ✅ HTTP estándar                     | ⚠️ Requiere configuración |

SSE es **HTTP estándar** — funciona a través de proxies, balanceadores y firewalls sin configuración especial. La reconexión automática está implementada nativamente en los browsers via `EventSource`. El overhead por mensaje es mayor que WebSockets (~200 bytes de headers HTTP vs ~2-10 bytes de frame WS), pero irrelevante para la frecuencia de actualización de este sistema (1 evento/60s por sensor).

El flujo de notificación es: `Projector → Redis Pub/Sub → API (subscrito) → SSE push a clientes`. Esto desacopla completamente el Projector de los clientes conectados.

---

## Consecuencias

**Positivas:**
- Implementación más simple que WebSockets — sin librería adicional, sin hub de conexiones.
- HTTP estándar — compatible con cualquier proxy/CDN sin configuración especial.
- Reconexión automática nativa en el browser.
- Modelo mental claro: un canal de solo lectura por shipment activo.

**Negativas:**
- No soporta comunicación bidireccional (no es un requerimiento, pero limita extensiones futuras).
- Limitado a 6 conexiones simultáneas por dominio en HTTP/1.1 (resuelto con HTTP/2 que multiplexa).

**Mitigación:** El servidor debe estar configurado con HTTP/2 para aprovechar el multiplexing. En HTTP/1.1, cada tab del browser abre una conexión SSE separada, lo que puede agotar el límite de conexiones por dominio si el usuario abre muchos envíos en paralelo.

---

## Alternativas descartadas

- **Polling REST:** No cumple NFR-3 a menos que el intervalo sea < 200ms, lo que genera carga innecesaria en el servidor. Descartado.
- **WebSockets / SignalR:** Correcto técnicamente, pero bidireccional cuando el caso de uso es unidireccional. Agrega complejidad operacional (hub de SignalR, gestión de grupos de conexiones, sticky sessions en balanceadores). Descartado por sobredimensionamiento.
