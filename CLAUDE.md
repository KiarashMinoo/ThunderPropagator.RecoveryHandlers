# CLAUDE.md

Guidance for working in this repository.

## Commands

```bash
dotnet restore    # fetches shared build configuration on first use; also needs read access to the core package feed
dotnet build
dotnet test
dotnet test <TestProject> --filter "FullyQualifiedName~<Name>"
dotnet clean      # also clears the downloaded shared build cache
```

## What this repo is

Pluggable recovery-storage backends: implementations of the core recovery-handler contract and its factory contract that persist and restore snapshot state to an external store, so a channel can recover after a restart. It contains no channels, feeders, or protocol handlers — only the storage backends.

## The backend pattern

Every backend implements the same trio, plus a feature-gate file:

- **`{Backend}RecoveryHandler`** — internal sealed (non-sealed in Debug), deriving from the shared recovery-handler base. Implements backup, restore (bulk and single-key), cleanup, and hibernate hooks. Override the async-initialize hook for setup that must happen before first use (e.g. schema creation, or resolving a shared connection from a cache).
- **`{Backend}RecoveryHandlerFactory`** — internal sealed, implementing the factory contract. Matches a channel by its configured storage kind; returns the real handler only if the backend's feature flag is allowed, otherwise a shared no-op fallback.
- **`{Backend}RecoveryHandlerExtensions`** — public static partial class with one DI-registration extension that wires up the factory and the feature flag.
- **Feature descriptor** — an internal feature type with a description attribute, gating whether the backend is usable at runtime.

## Shared connection/client caching

Clients that are expensive to construct per channel (and are documented by their driver as safe to share process-wide, owning their own pool/monitoring) get a singleton cache keyed by connection string, shared across every channel pointed at the same server — dedup in-flight connects, retry instead of permanently caching a failure, dispose everything on container shutdown. A handler never owns or disposes the shared client itself; only the cache does. Backends whose driver already manages pooling per-operation (open-per-call, pooled internally) don't need this pattern — don't add one speculatively.

## Architecture rules (enforced)

- Each backend assembly keeps its public/internal types inside its own namespace.
- No backend assembly may reference a sibling backend's namespace — backends must remain independently deployable/versionable.
- The whole backend assembly-reference graph must be acyclic.

## Conventions

- `internal sealed` (non-sealed in Debug via `#if !DEBUG sealed #endif`) for concrete handler classes.
- Source-generated logging methods for all logging — never pass a runtime-built string to a logger call; define one logging method per distinct compile-time message.
- All culture-sensitive parsing/formatting must use the invariant culture explicitly — recovery data is written on one machine and can be read back on another with a different OS locale.
- Guard-clause library for required connection strings — never suppress a null with the null-forgiving operator.

## Adding a backend

New backend area → the four-file trio above → register the connection/client cache if the client is expensive to construct → architecture-test rows for namespace containment and sibling isolation → unit tests covering DI registration and the backend's own serialize/round-trip behavior.

## Testing

xUnit, NSubstitute, FluentAssertions. A separate architecture-test project checks namespace containment, sibling-backend isolation, and the acyclic dependency graph. Both test projects have internal access to every backend assembly.

## Build & versioning

Version and target frameworks are centralized; CI bumps automatically on beta/release branches — never hand-edit during feature work. Package versions are centrally managed.
