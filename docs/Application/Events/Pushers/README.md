# Pushers

> Push event abstractions

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Base abstractions for events triggered during message push operations.

**Key Features:**
- 📤 Push event interface
- 🎯 Channel-specific events
- 📝 Event lifecycle hooks

## Public Types

### IPushEvent

**Kind:** Interface  
**Namespace:** `ThunderPropagator.Application.Events.Pushers`  
**Inherits:** `IEvent`

Interface for push events.

**Key Members:**
- `PushContext Context` - Push context
- `Task OnPushingAsync()` - Before push
- `Task OnPushedAsync()` - After push

**Usage Recipe:**
```csharp
public interface IMyPushEvent : IPushEvent
{
    string EventData { get; }
}
```

### AbstractPushEvent

**Kind:** Abstract Class  
**Namespace:** `ThunderPropagator.Application.Events.Pushers`  
**Implements:** `IPushEvent`

Base implementation for push events.

**Key Members:**
- `Guid Id` - Event ID
- `DateTime Timestamp` - Event timestamp
- `PushContext Context` - Context
- `virtual Task OnPushingAsync()` - Override for pre-push
- `virtual Task OnPushedAsync()` - Override for post-push

**Usage Recipe:**
```csharp
public class CompressionEvent : AbstractPushEvent
{
    protected override async Task OnPushingAsync()
    {
        // Compress before pushing
        if (Context.Message.Length > 1024)
        {
            var compressed = await CompressAsync(Context.Message);
            Context.Items["compressed"] = true;
        }
        
        await base.OnPushingAsync();
    }
    
    protected override async Task OnPushedAsync()
    {
        // Log after push
        if (Context.Items.ContainsKey("compressed"))
        {
            Logger.LogInformation("Sent compressed message");
        }
        
        await base.OnPushedAsync();
    }
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [IPushEvent.cs](../../src/ThunderPropagator.Application/Events/Pushers/IPushEvent.cs) | 14 | Push event interface |
| [AbstractPushEvent.cs](../../src/ThunderPropagator.Application/Events/Pushers/AbstractPushEvent.cs) | 23 | Base push event implementation |

**Total Files:** 2  
**Total LOC:** 37

## Usage

### Custom Push Event

```csharp
public class EncryptionEvent : AbstractPushEvent
{
    private readonly IEncryptionService _encryption;
    
    public EncryptionEvent(IEncryptionService encryption)
    {
        _encryption = encryption;
    }
    
    protected override async Task OnPushingAsync()
    {
        // Encrypt message before sending
        var encrypted = await _encryption.EncryptAsync(Context.Message.ToArray());
        Context.Items["encrypted"] = encrypted;
        Context.Items["original_size"] = Context.Message.Length;
        
        await base.OnPushingAsync();
    }
    
    protected override async Task OnPushedAsync()
    {
        // Log encryption metrics
        Logger.LogInformation(
            "Encrypted message: {OriginalSize} -> {EncryptedSize}",
            Context.Items["original_size"],
            ((byte[])Context.Items["encrypted"]).Length
        );
        
        await base.OnPushedAsync();
    }
}
```

---

**Navigation:**  
[⬆️ Events](../README.md) | [⬆️ Application Layer](../../README.md)

---

**Statistics:** 2 public types · 2 files · 37 LOC
