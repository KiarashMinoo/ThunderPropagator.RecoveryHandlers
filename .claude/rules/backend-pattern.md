---
paths:
  - "src/ThunderPropagator.RecoveryHandler.MongoDb/**"
  - "src/ThunderPropagator.RecoveryHandler.Postgresql/**"
  - "src/ThunderPropagator.RecoveryHandler.Redis/**"
---

# Backend Implementation Pattern

Every backend implements this trio + a feature-gate file:

- **`{Backend}RecoveryHandler`** — internal sealed (non-sealed in Debug), derives from the shared recovery-handler base. Backup, restore (bulk + single-key), cleanup, hibernate hooks. Override the async-initialize hook for pre-first-use setup (schema creation, resolving a shared connection from cache).
- **`{Backend}RecoveryHandlerFactory`** — internal sealed, implements the factory contract. Matches a channel by configured storage kind; returns the real handler only if the backend's feature flag allows it, else a shared no-op fallback.
- **`{Backend}RecoveryHandlerExtensions`** — public static partial class, one DI-registration extension wiring the factory + feature flag.
- **Feature descriptor** — internal feature type with a description attribute, gates runtime usability.

## Shared Connection/Client Caching

Clients expensive to construct per channel, and safe to share process-wide (own pool/monitoring): singleton cache keyed by connection string, shared across every channel pointed at the same server. Dedup in-flight connects, retry instead of caching a failure, dispose everything on container shutdown. A handler never owns/disposes the shared client — only the cache does. Skip this pattern for drivers that already pool per-operation.
