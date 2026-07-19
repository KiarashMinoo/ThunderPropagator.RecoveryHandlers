# SharedKernel

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types and members](#types-and-members)
- [Validation and constraints](#validation-and-constraints)
- [Package dependencies](#package-dependencies)
- [Diagrams](#diagrams)
- [Example](#example)
- [See also](#see-also)

## Overview

SharedKernel provides the small registration bridge used by every storage backend. It forwards feature registration to ThunderPropagator infrastructure while returning the original service collection for fluent configuration.

## Files

| File | Primary type(s)/symbol(s) | LOC (approx) | Responsibility |
|---|---|---:|---|
| `AssemblyInfo.cs` | Assembly attributes | 8 | Enables preview APIs and grants friend-assembly access |
| `ThunderPropagatorExtensions.cs` | `ThunderPropagatorExtensions` | 13 | Bridges backend feature registration into infrastructure |
| `ThunderPropagator.RecoveryHandler.SharedKernel.csproj` | Assembly manifest | 2 | Defines the shared package project |

## Types and members

| Type | Kind | Summary | Inherits/Implements | Key members |
|---|---|---|---|---|
| `ThunderPropagatorExtensions` | Public static partial class | Shared registration extensions for backend assemblies | Static class | Internal `AddFeature<TFeature>` |

### ThunderPropagatorExtensions

- **Kind:** Public static partial class
- **Namespace:** `ThunderPropagator.RecoveryHandler.SharedKernel`
- **Thread safety:** Stateless; thread safety follows the supplied `IServiceCollection`, which is normally configured during single-threaded application startup.
- **Serialization:** Not applicable.
- **Validation:** `TFeature` must be a reference type implementing `IFeature`.

Key method:

```csharp
internal static IServiceCollection AddFeature<TFeature>(
    this IServiceCollection services)
    where TFeature : class, IFeature
```

The method delegates to ThunderPropagator infrastructure and returns the same collection. It is internal because consumers register a concrete backend through that backend's public extension method.

#### Usage recipe

Backend packages call the bridge after registering an `IRecoveryHandlerFactory`:

```csharp
services.AddFeature<MyRecoveryStorageFeature>();
```

Application code should use a public backend extension such as `AddRedisRecoveryHandler()` instead.

[↑ Back to top](#contents)

## Validation and constraints

- Feature types must implement `IFeature`.
- Friend access is intentionally limited to the three backend assemblies and the two test assemblies.
- The assembly exposes no storage behavior and should remain independent of backend-specific drivers.

## Package dependencies

| Package | Version | Description | License / authors | Links |
|---|---:|---|---|---|
| `ThunderPropagator` | 1.0.1-beta.176 | Supplies `IFeature` and the infrastructure registration API | Apache-2.0; ThunderPropagator | [NuGet](https://www.nuget.org/packages/ThunderPropagator/1.0.1-beta.176) · [Repository](https://github.com/KiarashMinoo/ThunderPropagator) |

## Diagrams

### Registration bridge

```mermaid
graph LR
  Backend[Backend extension] --> Bridge[SharedKernel AddFeature]
  Bridge --> Infra[Infrastructure AddFeature]
  Infra --> DI[IServiceCollection]
```

SharedKernel keeps each provider's public registration method small and consistent.

## Example

```csharp
services
    .AddRedisRecoveryHandler()
    .AddMongoDbRecoveryHandler();
```

Each call uses the shared bridge to register its corresponding licensed feature.

## See also

- [Documentation portal](../README.md)
- [MongoDb](../MongoDb/README.md)
- [Postgresql](../Postgresql/README.md)
- [Redis](../Redis/README.md)

[↑ Back to top](#contents)
