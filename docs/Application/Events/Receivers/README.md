# Receivers

> Receive event abstractions

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Base abstractions for events triggered during request reception and processing.

**Key Features:**
- 📥 Receive event interface
- 🎯 Channel-specific events
- 📝 Event lifecycle hooks

## Public Types

### IReceiveEvent

**Kind:** Interface  
**Namespace:** `ThunderPropagator.Application.Events.Receivers`  
**Inherits:** `IEvent`

Interface for receive events.

**Key Members:**
- `ReceiveContext Context` - Request/response context
- `Task OnReceivingAsync()` - Before receive processing
- `Task OnReceivedAsync()` - After receive processing

**Usage Recipe:**
```csharp
public interface IMyReceiveEvent : IReceiveEvent
{
    string EventData { get; }
}
```

### AbstractReceiveEvent

**Kind:** Abstract Class  
**Namespace:** `ThunderPropagator.Application.Events.Receivers`  
**Implements:** `IReceiveEvent`

Base implementation for receive events.

**Key Members:**
- `Guid Id` - Event ID
- `DateTime Timestamp` - Event timestamp
- `ReceiveContext Context` - Context
- `virtual Task OnReceivingAsync()` - Override for pre-processing
- `virtual Task OnReceivedAsync()` - Override for post-processing

**Usage Recipe:**
```csharp
public class ValidationEvent : AbstractReceiveEvent
{
    protected override async Task OnReceivingAsync()
    {
        // Validate before processing
        if (!IsValid(Context.Request))
        {
            throw new ValidationException("Invalid request");
        }
        
        await base.OnReceivingAsync();
    }
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [IReceiveEvent.cs](../../src/ThunderPropagator.Application/Events/Receivers/IReceiveEvent.cs) | 14 | Receive event interface |
| [AbstractReceiveEvent.cs](../../src/ThunderPropagator.Application/Events/Receivers/AbstractReceiveEvent.cs) | 27 | Base receive event implementation |

**Total Files:** 2  
**Total LOC:** 41

## Usage

### Custom Receive Event

```csharp
public class AuthenticationEvent : AbstractReceiveEvent
{
    private readonly IAuthService _authService;
    
    public AuthenticationEvent(IAuthService authService)
    {
        _authService = authService;
    }
    
    protected override async Task OnReceivingAsync()
    {
        var token = Context.Request.Headers["Authorization"];
        var user = await _authService.ValidateTokenAsync(token);
        
        if (user == null)
        {
            Context.Response.StatusCode = 401;
            throw new UnauthorizedException();
        }
        
        Context.Request.Items["user"] = user;
        await base.OnReceivingAsync();
    }
}
```

---

**Navigation:**  
[⬆️ Events](../README.md) | [⬆️ Application Layer](../../README.md)

---

**Statistics:** 2 public types · 2 files · 41 LOC
