# RecoveryHandler.MongoDb

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types and Members](#types-and-members)
- [Validation and Constraints](#validation-and-constraints)
- [Performance Notes](#performance-notes)
- [Package Dependencies](#package-dependencies)
- [Diagrams](#diagrams)
- [Examples](#examples)
- [See Also](#see-also)

## Overview

The **RecoveryHandler.MongoDb** area groups 1 documented type, including `MongoDbRecoveryHandlerExtensions`. It provides the contracts and implementation used by this part of ThunderPropagator.RecoveryHandlers.

## Files

| File | Primary type(s)/symbol(s) | LOC (approx.) | Responsibility |
|---|---|---:|---|
| `AssemblyInfo.cs` | — | 6 | Contains the assembly info implementation or configuration. |
| `Features.cs` | `MongoDbRecoveryStorageFeature` | 15 | Defines MongoDbRecoveryStorageFeature and its related behavior. |
| `MongoClientCache.cs` | `MongoClientCache` | 48 | Defines MongoClientCache and its related behavior. |
| `MongoDbRecoveryHandler.cs` | `MongoDbRecoveryHandler`, `Log` | 151 | Defines MongoDbRecoveryHandler, Log and its related behavior. |
| `MongoDbRecoveryHandlerExtensions.cs` | `MongoDbRecoveryHandlerExtensions` | 20 | Defines MongoDbRecoveryHandlerExtensions and its related behavior. |
| `MongoDbRecoveryHandlerFactory.cs` | `MongoDbRecoveryHandlerFactory` | 22 | Defines MongoDbRecoveryHandlerFactory and its related behavior. |
| `ThunderPropagator.RecoveryHandler.MongoDb.csproj` | — | 8 | Defines project build targets, dependencies, and package metadata. |

## Types and Members

| Type | Kind | Summary | Inherits/Implements | Key Members |
|---|---|---|---|---|
| [`MongoDbRecoveryHandlerExtensions`](#mongodbrecoveryhandlerextensions) | class | Represents the MongoDbRecoveryHandlerExtensions class. | — | `AddMongoDbRecoveryHandler(…)` |

### MongoDbRecoveryHandlerExtensions

- **Kind:** class
- **Namespace:** `ThunderPropagator.RecoveryHandler.MongoDb`
- **Inherits/implements:** None declared
- **Attributes:** None detected
- **Key members:** `AddMongoDbRecoveryHandler(…)`
- **Summary:** Represents the MongoDbRecoveryHandlerExtensions class.
- **Thread safety:** Follow the lifetime and concurrency guarantees of the owning component; no additional guarantee is inferred.

**Usage recipe**

```csharp
// Resolve MongoDbRecoveryHandlerExtensions from the configured service container or construct it with its declared dependencies.
```

[↑ Back to top](#contents)

## Validation and Constraints

Inputs are validated at component boundaries. Callers should provide non-null required values and handle domain or argument exceptions without retrying invalid requests unchanged.

## Performance Notes

This area contains performance-sensitive constructs such as pooled buffers, spans, asynchronous value types, or concurrent collections. Avoid unnecessary allocations and blocking calls on streaming or message-processing paths.

## Package Dependencies

| Package | Version | Description | Links |
|---|---|---|---|
| `Dapper` | `2.1.79` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/Dapper) |
| `MongoDB.Driver` | `3.10.0` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/MongoDB.Driver) |
| `Npgsql` | `10.*` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/Npgsql) |
| `Pluralize.NET.Core` | `1.0.0` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/Pluralize.NET.Core) |
| `StackExchange.Redis` | `3.0.17` | External dependency used by the repository. | [Registry](https://www.nuget.org/packages/StackExchange.Redis) |

## Diagrams

### Component overview

```mermaid
graph TD
  Current["RecoveryHandler.MongoDb"]
  Current --> T0["MongoDbRecoveryHandlerExtensions"]
```

The diagram shows the direct components documented by the **RecoveryHandler.MongoDb** area.

## Examples

Start with `MongoDbRecoveryHandlerExtensions` as the primary entry point for this folder, then follow its linked contracts and collaborators.

## See Also

- [Documentation home](../README.md)
- [RecoveryHandler.Postgresql](../RecoveryHandler.Postgresql/README.md)
- [RecoveryHandler.Redis](../RecoveryHandler.Redis/README.md)
- [RecoveryHandler.SharedKernel](../RecoveryHandler.SharedKernel/README.md)

[↑ Back to top](#contents)
