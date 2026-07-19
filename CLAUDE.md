# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> **Note:** an earlier version of this file was a stale copy of the core `ThunderPropagator` repo's
> CLAUDE.md (it described channels, feeders, and WebSocket/MQTT/QUIC protocol handlers — none of
> which exist here). This version describes the actual contents of this repo.

## Build & Test Commands

```bash
# Restore (also downloads shared build props from ThunderPropagator.SharedBuild)
dotnet restore

# Build all projects
dotnet build

# Run all tests
dotnet test

# Run a specific test project
dotnet test Tests/ThunderPropagator.UnitTests/
dotnet test Tests/ThunderPropagator.ArchTests/

# Run a single test by name
dotnet test Tests/ThunderPropagator.UnitTests/ --filter "FullyQualifiedName~<TestMethodName>"

# Clean (removes .shared-props/ folder — next restore re-downloads them)
dotnet clean
```

**Important:** The first `dotnet restore` or `dotnet build` downloads `Shared.Build.props` and
`Shared.Nuget.props` from the `ThunderPropagator.SharedBuild` GitHub repo into `.shared-props/`. If
the build fails with `CS0246` type-not-found errors, re-run `dotnet restore` (network issue during
download). Restoring also requires read access to the `KiarashMinoo` GitHub Packages feed for the
`ThunderPropagator` and `ThunderPropagator.BuildingBlocks` package dependencies (`GH_TOKEN`).

## What this repo is

`ThunderPropagator.RecoveryHandlers` provides pluggable **recovery storage backends** for
`ThunderPropagator` channels — i.e. implementations of `IRecoveryHandler` /
`IRecoveryHandlerFactory` (contracts defined in the core `ThunderPropagator.Application` package)
that persist and restore `SnapshotEntry` state to an external store so a channel can recover after
a restart. It does **not** contain channels, feeders, or protocol handlers — those live in the core
`ThunderPropagator` and `ThunderPropagator.Channels` repos.

### Project Layout

```
src/
├── ThunderPropagator.RecoveryHandler.SharedKernel/   # AddFeature<T>() helper shared by all backends
├── ThunderPropagator.RecoveryHandler.Redis/          # StackExchange.Redis-backed recovery
├── ThunderPropagator.RecoveryHandler.MongoDb/        # MongoDB.Driver-backed recovery
└── ThunderPropagator.RecoveryHandler.Postgresql/     # Npgsql + Dapper-backed recovery

Tests/
├── ThunderPropagator.UnitTests/    # xUnit, NSubstitute, FluentAssertions; net10.0 only
└── ThunderPropagator.ArchTests/    # NetArchTest.Rules; namespace/dependency-direction enforcement
```

### The Recovery Handler Pattern

Each backend implements the same trio:

- **`{Backend}RecoveryHandler`** — `internal sealed partial class` deriving from
  `AbstractRecoveryHandler` (defined in core `ThunderPropagator.Application`). Implements
  `InternalBackupAsync`, `InternalRestoreAsync` (bulk and single-key overloads),
  `InternalCleanupAsync`, `InternalHibernateAsync`. Override `InternalInitializeAsync` for any
  setup that must happen asynchronously before first use (e.g. Postgres creates its schema/tables
  here; Redis/Mongo resolve their shared connection from a cache here — see below).
- **`{Backend}RecoveryHandlerFactory`** — `internal sealed class` implementing
  `IRecoveryHandlerFactory`. `CanHandle(channel)` matches on
  `channel.Metadata.Snapshot.RecoveryStorage`; `Create(channel)` returns the real handler only if
  `IFeatureGate.IsAllowed<TFeature>()` is true, otherwise returns the injected `NoneRecoveryHandler`
  singleton (feature-gated no-op fallback).
- **`{Backend}RecoveryHandlerExtensions`** — `public static partial class` with a single
  `services.Add{Backend}RecoveryHandler()` extension registering the factory and the feature flag.
- **`Features.cs`** — an `internal sealed class {Backend}RecoveryStorageFeature : IFeature` with a
  `[Description]` attribute, gating whether the backend is actually usable at runtime.

### Shared Connection/Client Caching

Redis and MongoDB clients are expensive to create per channel (`ConnectionMultiplexer` and
`MongoClient` are both documented by their respective drivers as processes-wide singletons that own
their own connection pool and background monitoring). Both backends therefore register a singleton
cache keyed by connection string, shared across every channel pointed at the same server:

- `RedisConnectionMultiplexerCache` (Redis project) — async, dedups in-flight connects via
  `Lazy<Task<IConnectionMultiplexer>>`, retries instead of permanently caching a failed connect,
  disposes all multiplexers when the DI container shuts down.
- `MongoClientCache` (MongoDb project) — sync (client construction doesn't block on I/O), same
  dedup-by-connection-string + centralized disposal pattern.

Neither handler owns or disposes its own client/multiplexer — the cache does. Do not add a
`DisposeManagedResourcesAsync` override that disposes the shared connection; that would break every
other channel sharing the same server.

PostgreSQL does not need this pattern — `NpgsqlConnection` is intentionally short-lived and opened
per operation via `OpenConnectionAsync()`, which is the correct usage for Npgsql/ADO.NET connections
(they're pooled internally by the driver already).

### Architecture Rules (Enforced by ArchTests)

- Each backend assembly (Redis, MongoDb, Postgresql, SharedKernel) must keep all its public/internal
  types within its own namespace (`Assemblies_WhenInspected_MustKeepTypesInExpectedNamespace`).
- No backend assembly may reference a sibling backend's namespace
  (`Assemblies_WhenInspected_MustNotDependOnSiblingBackends`) — Redis must not depend on MongoDb or
  Postgresql, etc. Backends must remain independently deployable/versionable.
- The recovery-handler assembly reference graph must be acyclic
  (`RecoveryHandlerAssemblies_WhenDependencyGraphIsInspected_MustBeAcyclic`).

### Code Conventions

- `internal sealed` (non-sealed in `DEBUG` via `#if !DEBUG sealed #endif`) for concrete handler
  classes, matching the pattern used across every `ThunderPropagator.*` repo.
- `[LoggerMessage]` source-generated partial methods for all logging — never
  `Logger.LogError(exception, someRuntimeString)`; if the message varies by call site, define one
  `[LoggerMessage]` method per distinct compile-time message and pass the method group (or an
  `Action<ILogger, Exception>` delegate) rather than a runtime string.
- All culture-sensitive parsing/formatting (numeric types, `DateTime`) must use
  `CultureInfo.InvariantCulture` explicitly — recovery data is serialized on one machine and can be
  deserialized on another with a different OS locale.
- `Guard.Against.NullOrWhiteSpace(...)` (Ardalis.GuardClauses) for required connection strings —
  never suppress a null connection string with the `!` null-forgiving operator.

### Build Infrastructure

`Directory.Build.props` is the single source of truth for versioning (`<Version>`) and target
frameworks (`net8.0;net9.0;net10.0`). It downloads two shared property files at restore/build time:

| File | Purpose |
|---|---|
| `.shared-props/Shared.Build.props` | SDK-wide settings: TFM, nullable, warnings-as-errors, etc. |
| `.shared-props/Shared.Nuget.props` | NuGet metadata: authors, license, icon, package tags |

`Directory.Packages.props` manages all NuGet dependency versions centrally (CPM). Add new packages
there; never specify `Version` on individual `<PackageReference>` items.

### Versioning

The version is set in `Directory.Build.props` under `<Version>`. The CI pipeline bumps this
automatically on beta/release branches. Do not edit it manually during feature work.

### Solution File

The solution uses the `.slnx` format (`ThunderPropagator.RecoveryHandlers.slnx`) required by
JetBrains Rider. There is no legacy `.sln` file.

### Testing

- `ThunderPropagator.UnitTests` — xUnit, NSubstitute, FluentAssertions; targets `net10.0` only.
  Covers DI registration, `PostgresqlSnapshotTypeMapper` serialize/deserialize (including
  culture round-trip and unresolvable-type failure cases), and the Redis/Mongo connection caches.
- `ThunderPropagator.ArchTests` — NetArchTest.Rules; namespace containment, sibling-backend
  isolation, and acyclic dependency graph checks.
- Both test projects have `InternalsVisibleTo` access to all four `src/` assemblies.
