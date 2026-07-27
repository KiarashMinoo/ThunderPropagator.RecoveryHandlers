# ThunderPropagator.RecoveryHandlers Documentation

Pluggable snapshot-recovery backends for ThunderPropagator using Redis, MongoDB, and PostgreSQL.

## Contents

- [Documentation areas](#documentation-areas)
- [Package dependencies](#package-dependencies)
- [Coverage audit](#coverage-audit)

## Documentation areas

- [RecoveryHandler.MongoDb](./RecoveryHandler.MongoDb/README.md) `Types:1` `Files:7` `Diagrams:✓`
- [RecoveryHandler.Postgresql](./RecoveryHandler.Postgresql/README.md) `Types:1` `Files:12` `Diagrams:✓`
- [RecoveryHandler.Redis](./RecoveryHandler.Redis/README.md) `Types:1` `Files:7` `Diagrams:✓`
- [RecoveryHandler.SharedKernel](./RecoveryHandler.SharedKernel/README.md) `Types:1` `Files:3` `Diagrams:✓`

## Package dependencies

| Package | Version | Registry |
|---|---|---|
| `Dapper` | `2.1.79` | [Package](https://www.nuget.org/packages/Dapper) |
| `MongoDB.Driver` | `3.10.0` | [Package](https://www.nuget.org/packages/MongoDB.Driver) |
| `Npgsql` | `10.*` | [Package](https://www.nuget.org/packages/Npgsql) |
| `Pluralize.NET.Core` | `1.0.0` | [Package](https://www.nuget.org/packages/Pluralize.NET.Core) |
| `StackExchange.Redis` | `3.0.17` | [Package](https://www.nuget.org/packages/StackExchange.Redis) |

## Coverage audit

| Documentation area | Status | Files | Types | Retry passes |
|---|---|---:|---:|---:|
| [`RecoveryHandler.MongoDb`](./RecoveryHandler.MongoDb/README.md) | ✅ Complete | 7 | 1 | 1 |
| [`RecoveryHandler.Postgresql`](./RecoveryHandler.Postgresql/README.md) | ✅ Complete | 12 | 1 | 1 |
| [`RecoveryHandler.Redis`](./RecoveryHandler.Redis/README.md) | ✅ Complete | 7 | 1 | 1 |
| [`RecoveryHandler.SharedKernel`](./RecoveryHandler.SharedKernel/README.md) | ✅ Complete | 3 | 1 | 1 |

**Last generated:** July 27, 2026
