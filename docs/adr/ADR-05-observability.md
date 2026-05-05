# ADR-05: Backend de Observabilidad

**Estado:** Aceptado  
**Fecha:** 2026-04-28  
**Autores:** Equipo CryoLogistics

---

## Contexto

El sistema es multi-proceso (.NET API + Projector Worker + Excursion Engine Worker). Se requiere observabilidad sobre tres señales — **trazas distribuidas**, **métricas** y **logs estructurados** — con correlación entre ellas para diagnosticar problemas de latencia o pérdida de eventos.

Se evaluó la capa de instrumentación separadamente del backend de almacenamiento y visualización, dado que son decisiones independientes.

---

## Decisión

### Capa de instrumentación: OpenTelemetry

Se usa **OpenTelemetry** como única capa de instrumentación. El exportador (`OtlpExporter`) se configura via variables de entorno — cambiar el backend destino no requiere modificar código de la aplicación.

### Backend de desarrollo: .NET Aspire Dashboard

Se usa el **Aspire Dashboard** como backend de observabilidad para el entorno de desarrollo local.

---

## Justificación

| Criterio                             | Aspire Dashboard                           | Prometheus + Grafana + Loki + Tempo               |
| ------------------------------------ | ------------------------------------------ | ------------------------------------------------- |
| **Protocolo de ingesta**             | ✅ OTLP nativo                              | ⚠️ Requiere OTEL Collector como intermediario      |
| **Configuración inicial**            | ✅ Zero-config — un contenedor Docker       | ❌ 4+ servicios (Prometheus, Grafana, Loki, Tempo) |
| **Correlación trazas/logs/métricas** | ✅ Integrada en una UI                      | ✅ Integrada en Grafana                            |
| **Persistencia de señales**          | ❌ Solo en memoria — se pierde al reiniciar | ✅ Persistencia en disco configurable              |
| **Adecuación a desarrollo local**    | ✅ Exactamente lo necesario                 | ⚠️ Sobredimensionado para desarrollo               |
| **Adecuación a producción**          | ❌ Sin persistencia, sin alertas            | ✅ Stack completo                                  |

Para desarrollo local, el objetivo es observar lo que acaba de ocurrir durante una sesión de trabajo — no mantener historia. La falta de persistencia del Aspire Dashboard no es un problema en ese contexto. Agregar Prometheus + Grafana + Loki + Tempo para desarrollo implicaría la misma complejidad operacional que producción sin beneficio adicional.

El Aspire Dashboard **no requiere adoptar el modelo de hosting completo de .NET Aspire** — funciona como un contenedor Docker independiente que recibe señales OTLP.

---

## Consecuencias

**Positivas:**
- Zero-config: un solo contenedor Docker recibe trazas, métricas y logs via OTLP.
- UI integrada con correlación entre las tres señales desde el primer día de desarrollo.
- Sin dependencia de librerías o configuración adicional en el código de la aplicación.
- El mismo `OtlpExporter` apuntará al backend de producción cambiando solo la variable de entorno.

**Negativas:**
- Sin persistencia — las señales se pierden al reiniciar el contenedor del dashboard.
- Sin capacidad de alertas — no reemplaza el stack de producción.

---

## Alternativas descartadas

- **Jaeger:** Especializado solo en trazas. Requeriría herramientas adicionales para métricas y logs sin correlación nativa entre ellas. Descartado.
- **Seq:** Excelente para logs estructurados en .NET, pero no cubre métricas ni trazas. Descartado.
- **Zipkin:** Más simple que Jaeger, ecosistema más limitado. Descartado.
- **SaaS (Grafana Cloud, Datadog, Azure Monitor):** Descartados — el sistema debe ser independiente de infraestructura cloud o proveedor externo.
