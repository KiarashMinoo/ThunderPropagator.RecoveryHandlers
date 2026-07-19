# Pushers

> Push pipeline abstractions for message transformation

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Push pipelines process outbound messages using chain-of-responsibility pattern, enabling message transformation, compression, encryption, and formatting before protocol transmission.

**Key Features:**
- 🔗 Pipeline chaining
- 📤 Message transformation
- 🔒 Encryption/compression hooks
- 📝 Logging support

## Public Types

### IPushPipeline

**Kind:** Interface  
**Namespace:** `ThunderPropagator.Application.Pipelines.Pushers`

Interface for push pipelines.

**Key Members:**
- `Task InvokeAsync(PushContext, PushPipelineDelegate)` - Process message

**Usage Recipe:**
```csharp
public class MyPushPipeline : IPushPipeline
{
    public async Task InvokeAsync(PushContext context, PushPipelineDelegate next)
    {
        // Transform
        await next(context);
    }
}
```

### AbstractPushPipeline

**Kind:** Abstract Class  
**Namespace:** `ThunderPropagator.Application.Pipelines.Pushers`  
**Implements:** `IPushPipeline`

Base push pipeline with logging support.

**Key Members:**
- `ILogger Logger` - Logger instance
- `PushContext Context` - Current context

**Usage Recipe:**
```csharp
public class CompressionPipeline : AbstractPushPipeline<MyChannel>
{
    public CompressionPipeline(ILoggerFactory loggerFactory) 
        : base(loggerFactory)
    {
    }
    
    public async Task InvokeAsync(PushContext context, PushPipelineDelegate next)
    {
        if (context.Message.Length > 1024)
        {
            var compressed = Compress(context.Message);
            Logger.LogInformation(
                "Compressed {Original} to {Compressed} bytes",
                context.Message.Length,
                compressed.Length
            );
        }
        
        await next(context);
    }
}
```

### PushPipelineDelegate

**Kind:** Delegate  
**Namespace:** `ThunderPropagator.Application.Pipelines.Pushers`

Delegate for push pipeline chaining.

**Signature:**
```csharp
public delegate Task PushPipelineDelegate(PushContext context);
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [IPushPipeline.cs](../../src/ThunderPropagator.Application/Pipelines/Pushers/IPushPipeline.cs) | 14 | Push pipeline interface |
| [AbstractPushPipeline.cs](../../src/ThunderPropagator.Application/Pipelines/Pushers/AbstractPushPipeline.cs) | 14 | Base pipeline with logging |
| [PushPipelineDelegate.cs](../../src/ThunderPropagator.Application/Pipelines/Pushers/PushPipelineDelegate.cs) | 6 | Pipeline delegate |

**Total Files:** 3  
**Total LOC:** 34

## Usage

### Creating Push Pipeline

```csharp
public class EncryptionPipeline : AbstractPushPipeline<SecureChannel>
{
    private readonly IEncryptionService _encryption;
    
    public EncryptionPipeline(
        ILoggerFactory loggerFactory,
        IEncryptionService encryption) 
        : base(loggerFactory)
    {
        _encryption = encryption;
    }
    
    public async Task InvokeAsync(PushContext context, PushPipelineDelegate next)
    {
        // Encrypt message
        var encrypted = await _encryption.EncryptAsync(context.Message.ToArray());
        
        Logger.LogDebug(
            "Encrypted message for {ConnectionId}",
            context.ConnectionInfo.ConnectionId
        );
        
        context.Items["encrypted"] = true;
        context.Items["original_size"] = context.Message.Length;
        
        await next(context);
    }
}
```

### Pipeline Chaining

```csharp
// Register pipelines
services.AddSingleton<IPushPipeline, LoggingPipeline>();
services.AddSingleton<IPushPipeline, CompressionPipeline>();
services.AddSingleton<IPushPipeline, EncryptionPipeline>();

// Execution flow:
// Message → LoggingPipeline → CompressionPipeline → EncryptionPipeline → Protocol Send
```

### Multi-Stage Transformation

```csharp
public class TransformationPipeline : AbstractPushPipeline<DataChannel>
{
    public TransformationPipeline(ILoggerFactory loggerFactory) 
        : base(loggerFactory)
    {
    }
    
    public async Task InvokeAsync(PushContext context, PushPipelineDelegate next)
    {
        // Step 1: Compress
        if (context.Message.Length > 1024)
        {
            var compressed = await CompressAsync(context.Message);
            context.Items["compressed"] = true;
            Logger.LogDebug("Message compressed");
        }
        
        // Step 2: Encrypt
        if (context.Channel.Metadata.MessageEncryption?.IsEnabled == true)
        {
            var encrypted = await EncryptAsync(context.Message);
            context.Items["encrypted"] = true;
            Logger.LogDebug("Message encrypted");
        }
        
        // Step 3: Add checksum
        var checksum = CalculateChecksum(context.Message);
        context.Items["checksum"] = checksum;
        
        await next(context);
    }
}
```

---

**Navigation:**  
[⬆️ Pipelines](../README.md) | [⬆️ Application Layer](../../README.md)

---

**Statistics:** 3 public types · 3 files · 34 LOC
