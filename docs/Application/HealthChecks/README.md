# HealthChecks

> Health check support abstractions for monitoring component status

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Provides interfaces for integrating ThunderPropagator components with ASP.NET Core health check system. Channels, feeders, and protocol containers implement these interfaces to report their operational status.

**Key Features:**
- 🏥 ASP.NET Core health check integration
- 📊 Component health monitoring
- 🏷️ Tag-based health grouping
- ⚠️ Exception reporting

## Public Types

### IHealthCheckSupport

**Kind:** Interface  
**Namespace:** `ThunderPropagator.Application.HealthChecks`

Interface for components supporting health checks.

**Key Members:**
- `IEnumerable<string> HealthTags` - Tags for health check grouping
- `HealthStatus HealthStatus` - Current health status
- `Exception? HealthException` - Health check exception (if unhealthy)
- `void ReportHealth(HealthStatus, Exception?)` - Report health status

**Usage Recipe:**
```csharp
public class MyComponent : IHealthCheckSupport
{
    public IEnumerable<string> HealthTags { get; } = ["mycomponent"];
    public HealthStatus HealthStatus { get; private set; } = HealthStatus.Healthy;
    public Exception? HealthException { get; private set; }
    
    public void ReportHealth(HealthStatus status, Exception? exception = null)
    {
        HealthStatus = status;
        HealthException = exception;
    }
    
    public void DoWork()
    {
        try
        {
            // Work logic
            ReportHealth(HealthStatus.Healthy);
        }
        catch (Exception ex)
        {
            ReportHealth(HealthStatus.Unhealthy, ex);
        }
    }
}
```

### IHealthCheckSupportHandler

**Kind:** Interface (Internal)  
**Namespace:** `ThunderPropagator.Application.HealthChecks`

Internal handler interface for health check collection and aggregation.

**Key Members:**
- `Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext, CancellationToken)` - Execute health check

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [IHealthCheckSupport.cs](../../src/ThunderPropagator.Application/HealthChecks/IHealthCheckSupport.cs) | 12 | Health check support interface |
| [IHealthCheckSupportHandler.cs](../../src/ThunderPropagator.Application/HealthChecks/IHealthCheckSupportHandler.cs) | 15 | Internal health check handler |

**Total Files:** 2  
**Total LOC:** 27

## Usage

### Implementing Health Check Support

```csharp
public class MyFeeder : AbstractFeeder<MyChannel, MyMessage, MyConfig>
{
    // AbstractFeeder already implements IHealthCheckSupport
    // HealthTags initialized in base constructor
    
    protected override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _connection.OpenAsync(cancellationToken);
            ReportHealth(HealthStatus.Healthy);
        }
        catch (Exception ex)
        {
            ReportHealth(HealthStatus.Unhealthy, ex);
            throw;
        }
    }
}
```

### Registering Health Checks

```csharp
// In Infrastructure layer
services.AddHealthChecks()
    .AddCheck<HealthCheckSupportHandler>(
        "thunderpropagator",
        tags: new[] { "ready", "live" }
    );
```

---

**Navigation:**  
[⬆️ Application Layer](../README.md)

---

**Statistics:** 2 public types · 2 files · 27 LOC
