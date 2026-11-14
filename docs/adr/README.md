# ADRs — Índice de Decisiones de Arquitectura

> [← README](../../README.md)

Los ADRs documentan las decisiones de arquitectura significativas: el contexto en que se tomaron, las alternativas consideradas y las consecuencias esperadas. Siguen el formato de [Michael Nygard](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions).

---

| ADR                                                 | Título                                                                 | Estado     |
| --------------------------------------------------- | ---------------------------------------------------------------------- | ---------- |
| [ADR-01](ADR-01-eventstore.md)                      | Event Store propio sobre PostgreSQL vs Marten / EventStoreDB           | ✅ Aceptado |
| [ADR-02](ADR-02-kafka.md)                           | Apache Kafka como bus de eventos vs RabbitMQ vs Redis Streams          | ✅ Aceptado |
| [ADR-03](ADR-03-sse.md)                             | SSE (Server-Sent Events) vs WebSockets / SignalR para push al frontend | ✅ Aceptado |
| [ADR-04](ADR-04-keycloak.md)                        | Keycloak como Identity Provider vs Azure AD vs IdentityServer          | ✅ Aceptado |
| [ADR-05](ADR-05-observability.md)                   | Backend de Observabilidad                                              | ✅ Aceptado |
| [ADR-06](ADR-06-projector-idempotency.md)           | Estrategia de idempotencia del Projector                               | ✅ Aceptado |
| [ADR-07](ADR-07-snapshotting.md)                    | Estrategia de Snapshotting del Agregado                                | ✅ Aceptado |
| [ADR-08](ADR-08-eventual-consistency.md)            | Garantía de Consistencia Eventual Write/Read para usuario QA           | ✅ Aceptado |
| [ADR-09](ADR-09-data-retention.md)                  | Estrategia de retención de datos: Hot / Warm / Cold                    | ✅ Aceptado |
| [ADR-10](ADR-10-outbox-inbox-integration-events.md) | Publicación Garantizada de Eventos de Integración (Outbox / Inbox)     | ✅ Aceptado |
| [ADR-11](ADR-11-ci-devsecops.md)                    | CI / DevSecOps con GitHub Actions y SonarCloud                         | ✅ Aceptado |
