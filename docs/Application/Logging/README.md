# Logging

> Logging utilities and helpers

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Utility helpers for standardized logging across ThunderPropagator components.

**Key Features:**
- 📝 Structured logging helpers
- 🎯 Consistent log formatting
- 🔍 Context enrichment

## Public Types

### LoggerHelper

**Kind:** Static Class  
**Namespace:** `ThunderPropagator.Application.Logging`

Helper methods for consistent logging patterns.

**Key Members:**
- `static void LogFeederState(ILogger logger, string feederName, FeederState state)` - Log feeder state change
- `static void LogChannelOperation(ILogger logger, string channelName, string operation)` - Log channel operation

**Usage Recipe:**
```csharp
public class MyFeeder : AbstractFeeder<MyChannel, MyMessage, MyConfig>
{
    protected override Task StartedAsync(CancellationToken cancellationToken)
    {
        LoggerHelper.LogFeederState(Logger, GetType().Name, FeederState.Started);
        return Task.CompletedTask;
    }
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [LoggerHelper.cs](../../src/ThunderPropagator.Application/Logging/LoggerHelper.cs) | 31 | Logging utility methods |

**Total Files:** 1  
**Total LOC:** 31

## Usage

### Structured Logging

```csharp
public class MyComponent
{
    private readonly ILogger _logger;
    
    public void DoWork()
    {
        LoggerHelper.LogChannelOperation(_logger, "stocks", "message_emit");
    }
}
```

---

**Navigation:**  
[⬆️ Application Layer](../README.md)

---

**Statistics:** 1 public type · 1 file · 31 LOC
