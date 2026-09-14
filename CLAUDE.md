# CLAUDE.md

<!-- Humans: keep this file lean — per-backend implementation patterns live in .claude/rules/, not here. -->

## Commands

```bash
dotnet restore    # fetches shared build config; also needs read access to the core package feed
dotnet build
dotnet test
dotnet test <TestProject> --filter "FullyQualifiedName~<Name>"
dotnet clean      # also clears the downloaded shared build cache
```

## What This Repo Is

Pluggable recovery-storage backends: implementations of the core recovery-handler contract + factory contract, persisting/restoring snapshot state so a channel can recover after a restart. No channels, feeders, or protocol handlers — storage backends only.

Per-backend implementation trio + shared connection/client caching pattern: `.claude/rules/backend-pattern.md`.

## Architecture Rules (Enforced)

- Each backend assembly's public/internal types stay in its own namespace.
- No backend assembly references a sibling backend's namespace.
- The backend assembly-reference graph is acyclic.

## Conventions

- `internal sealed` (non-sealed in Debug via `#if !DEBUG sealed #endif`) for concrete handler classes.
- Source-generated logging methods only — never a runtime-built string to a logger call; one logging method per distinct compile-time message.
- Invariant culture explicitly for all culture-sensitive parsing/formatting.
- Guard-clause library for required connection strings — never null-forgiving operator.

## Adding a Backend

New backend area → the four-file trio → register connection/client cache if the client is expensive to construct → architecture-test rows (namespace containment, sibling isolation) → unit tests (DI registration, serialize/round-trip).

## Testing

xUnit, NSubstitute, FluentAssertions. Architecture-test project checks namespace containment, sibling isolation, acyclic graph. Both test projects have internal access to every backend assembly.

## Build & Versioning

Version/TFMs centralized; CI bumps automatically on beta/release branches — never hand-edit during feature work. Package versions centrally managed.
