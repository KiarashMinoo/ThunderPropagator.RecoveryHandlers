# ThunderPropagator Recovery Handlers

Pluggable snapshot-recovery backends for ThunderPropagator. This repository provides MongoDB, PostgreSQL, and Redis implementations that integrate through the common `IRecoveryHandlerFactory` contract and fall back to the built-in no-op handler when a storage feature is not licensed.

The packages target .NET 8, .NET 9, and .NET 10 on AnyCPU, x86, x64, and ARM64.

## Contents

- [Packages](#packages)
- [Quick start](#quick-start)
- [Build](#build)
- [Package sources](#package-sources)
- [Documentation](#documentation)
- [License](#license)

## Packages

| Package | Storage | Registration |
|---|---|---|
| `ThunderPropagator.RecoveryHandler.MongoDb` | MongoDB collection per channel | `AddMongoDbRecoveryHandler()` |
| `ThunderPropagator.RecoveryHandler.Postgresql` | PostgreSQL schema and tables per channel | `AddPostgresqlRecoveryHandler()` |
| `ThunderPropagator.RecoveryHandler.Redis` | Redis hash per channel | `AddRedisRecoveryHandler()` |
| `ThunderPropagator.RecoveryHandler.SharedKernel` | Shared feature-registration support | Registered transitively |

## Quick start

Install one backend package, configure the channel snapshot metadata with the matching recovery storage and connection string, and register the handler:

```csharp
using ThunderPropagator.RecoveryHandler.Postgresql;

services.AddPostgresqlRecoveryHandler();
```

The recovery factory selects this handler only for channels configured with `RecoveryStorage.Postgresql`. If the associated feature is unavailable, the factory returns `NoneRecoveryHandler`.

## Build

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

The build downloads shared MSBuild properties into `.shared-props` when they are absent.

## Package sources

The repository's `NuGet.Config` currently uses the public NuGet v3 feed:

```text
https://api.nuget.org/v3/index.json
```

To consume organization packages from GitHub Packages when required, add the source without committing credentials:

```bash
dotnet nuget add source https://nuget.pkg.github.com/KiarashMinoo/index.json \
  --name GitHub-KiarashMinoo \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_TOKEN \
  --store-password-in-clear-text
```

## Documentation

<!-- repo-docs:start -->
Generated documentation is available in the [documentation portal](docs/README.md).

- [MongoDb](docs/MongoDb/README.md) `Types:1` `Files:6` `Diagrams:✓`
- [Postgresql](docs/Postgresql/README.md) `Types:1` `Files:12` `Diagrams:✓`
- [Redis](docs/Redis/README.md) `Types:1` `Files:6` `Diagrams:✓`
- [SharedKernel](docs/SharedKernel/README.md) `Types:1` `Files:3` `Diagrams:✓`

### Direct package dependencies

| Package | Configured version | Used by |
|---|---:|---|
| [Dapper](https://www.nuget.org/packages/Dapper/2.1.79) | 2.1.79 | [Postgresql](docs/Postgresql/README.md#package-dependencies) |
| [MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver/3.10.0) | 3.10.0 | [MongoDb](docs/MongoDb/README.md#package-dependencies) |
| [Npgsql](https://www.nuget.org/packages/Npgsql) | 8.0.9 / 9.0.5 / 10.0.3 | [Postgresql](docs/Postgresql/README.md#package-dependencies) |
| [Pluralize.NET.Core](https://www.nuget.org/packages/Pluralize.NET.Core/1.0.0) | 1.0.0 | [Postgresql](docs/Postgresql/README.md#package-dependencies) |
| [StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis/3.0.17) | 3.0.17 | [Redis](docs/Redis/README.md#package-dependencies) |
| `ThunderPropagator` | 1.0.1-beta.176 | All assemblies |

**Last generated:** July 19, 2026
<!-- repo-docs:end -->

## License

Packages produced by this repository declare the Apache-2.0 license.
