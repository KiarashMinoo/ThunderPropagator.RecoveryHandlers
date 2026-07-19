# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

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

# Run a single test by name
dotnet test Tests/ThunderPropagator.UnitTests/ --filter "FullyQualifiedName~<TestMethodName>"

# Clean (removes .shared-props/ folder — next restore re-downloads them)
dotnet clean
```

**Important:** The first `dotnet restore` or `dotnet build` downloads `Shared.Build.props` and `Shared.Nuget.props` from the `ThunderPropagator.SharedBuild` GitHub repo into `.shared-props/`. If the build fails with `CS0246` type-not-found errors, re-run `dotnet restore` (network issue during download).

## Architecture

ThunderPropagator is a .NET library for real-time data streaming. It ships as two NuGet packages from GitHub Packages:

- **ThunderPropagator.Application** — contracts, abstractions, and domain logic
- **ThunderPropagator.Infrastructure** — protocol implementations and service wiring

Targets: `net8.0`, `net9.0`, `net10.0` | Platforms: `ARM64`, `x64`, `x86`, `AnyCPU`

### Layer Responsibilities

**Application layer** (`src/ThunderPropagator.Application/`) defines the public API:
- `IChannel` / `AbstractChannel` — channel lifecycle, subscription management, health checks, and recovery
- `IFeeder` / `AbstractFeeder` — data sources that feed into channels
- `Subscription`, `SubscribedFields`, `SubscribedKeys` — fine-grained subscription contracts
- `IRecoveryHandler` + `SnapshotEntry` — pluggable state persistence interface
- Pipeline middleware contracts (request/response processing)

**Infrastructure layer** (`src/ThunderPropagator.Infrastructure/`) provides:
- Protocol handlers: WebSockets, MQTT 5.0 (`MQTTnet`), QUIC, WebTransport (HTTP/3), InfiniteDataStream
- Recovery implementations backed by Redis, MongoDB, PostgreSQL, or in-memory (None)
- Receiver pipeline steps: authentication (Basic/OAuth2), authorization, PingPong health, Subscribe/Unsubscribe
- Pusher pipeline for outbound message delivery
- DI registration via extension methods in `Extensions/`

### Key Patterns

- **Strict layer separation**: Application has no dependency on Infrastructure. Infrastructure references Application for contracts only.
- **Pluggable recovery**: Swap persistence backends (Redis / MongoDB / Postgres / None) without changing channel logic.
- **Pipeline composition**: Both receive and push paths are built from ordered middleware steps, each handling a single concern (auth, authorization, routing, etc.).
- **Multi-protocol fan-out**: A single channel subscription can push to multiple protocol handlers simultaneously.

### Build Infrastructure

`Directory.Build.props` is the single source of truth for versioning (`<Version>`) and target frameworks. It downloads two shared property files at restore/build time:

| File | Purpose |
|---|---|
| `.shared-props/Shared.Build.props` | SDK-wide settings: TFM, nullable, warnings-as-errors, etc. |
| `.shared-props/Shared.Nuget.props` | NuGet metadata: authors, license, icon, package tags |

`Directory.Packages.props` manages all NuGet dependency versions centrally (CPM). Add new packages there; never specify `Version` on individual `<PackageReference>` items.

### Versioning

The version is set in `Directory.Build.props` under `<Version>`. The CI pipeline bumps this automatically on beta/release branches. Do not edit it manually during feature work.

### Solution File

The solution uses the `.slnx` format (`ThunderPropagator.RecoveryHandlers.slnx`) required by JetBrains Rider. The legacy `ThunderPropagator.RecoveryHandlers.sln` has been removed.

### Testing

- `ThunderPropagator.UnitTests` — xUnit, NSubstitute; targets `net10.0` only
- `ThunderPropagator.ArchTests` — NetArchTest.Rules; architecture constraint tests (currently being scaffolded)
- Both test projects have `InternalsVisibleTo` access to Application and Infrastructure internals
