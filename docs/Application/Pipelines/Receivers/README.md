# Receivers

> Receive pipeline abstractions for request processing

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Submodules](#submodules)
- [Usage](#usage)

## Overview

Receive pipelines process incoming requests using chain-of-responsibility pattern, enabling modular request handling, validation, authentication, and authorization.

**Key Features:**
- 🔗 Pipeline chaining
- 🎯 Request key routing
- 📝 Attribute-based documentation
- ⚠️ Exception handling

## Public Types

### IReceivePipeline

**Kind:** Interface  
**Namespace:** `ThunderPropagator.Application.Pipelines.Receivers`

Interface for receive pipelines.

**Key Members:**
- `string RequestKey` - Request action/command key
- `Task InvokeAsync(ReceiveContext, ReceivePipelineDelegate)` - Process request

**Usage Recipe:**
```csharp
public class MyPipeline : IReceivePipeline
{
    public string RequestKey => "my_action";
    
    public async Task InvokeAsync(ReceiveContext context, ReceivePipelineDelegate next)
    {
        // Process
        await next(context);
    }
}
```

### AbstractReceivePipeline

**Kind:** Abstract Class  
**Namespace:** `ThunderPropagator.Application.Pipelines.Receivers`  
**Implements:** `IReceivePipeline`

Base receive pipeline with logging support.

**Key Members:**
- `ILogger Logger` - Logger instance
- `ReceiveContext Context` - Current context
- `RequestContext Request` - Request context
- `ResponseContext Response` - Response context
- `abstract string RequestKey` - Override to specify request key

**Usage Recipe:**
```csharp
public class ValidationPipeline : AbstractReceivePipeline<MyChannel>
{
    public override string RequestKey => "subscribe";
    
    public ValidationPipeline(ILoggerFactory loggerFactory) 
        : base(loggerFactory)
    {
    }
    
    public async Task InvokeAsync(ReceiveContext context, ReceivePipelineDelegate next)
    {
        // Validate
        if (!IsValid(Request))
        {
            Response.StatusCode = 400;
            return;
        }
        
        Logger.LogInformation("Request validated");
        await next(context);
    }
}
```

### ReceivePipelineDelegate

**Kind:** Delegate  
**Namespace:** `ThunderPropagator.Application.Pipelines.Receivers`

Delegate for pipeline chaining.

**Signature:**
```csharp
public delegate Task ReceivePipelineDelegate(ReceiveContext context);
```

### ReceivePipelineRequestResponseMetadata

**Kind:** Record  
**Namespace:** `ThunderPropagator.Application.Pipelines.Receivers`

Metadata for request/response schemas.

**Key Members:**
- `Type? RequestType` - Request DTO type
- `Type? ResponseType` - Response DTO type

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [IReceivePipeline.cs](../../src/ThunderPropagator.Application/Pipelines/Receivers/IReceivePipeline.cs) | 13 | Receive pipeline interface |
| [AbstractReceivePipeline.cs](../../src/ThunderPropagator.Application/Pipelines/Receivers/AbstractReceivePipeline.cs) | 29 | Base pipeline with logging |
| [ReceivePipelineDelegate.cs](../../src/ThunderPropagator.Application/Pipelines/Receivers/ReceivePipelineDelegate.cs) | 6 | Pipeline delegate |
| [ReceivePipelineRequestResponseMetadata.cs](../../src/ThunderPropagator.Application/Pipelines/Receivers/ReceivePipelineRequestResponseMetadata.cs) | 4 | Request/response metadata |

**Total Files (excluding Attributes):** 4  
**Total LOC:** 52

## Submodules

- [📁 Attributes](Attributes/README.md) - Pipeline documentation attributes (55 LOC, 3 files)

## Usage

### Creating Receive Pipeline

```csharp
[ReceivePipelineRequestSchema(typeof(SubscribeRequest))]
[ReceivePipelineResponseSchema(typeof(SubscribeResponse))]
[ReceivePipelineException(typeof(InvalidSubscriptionException), 400)]
public class SubscribePipeline : AbstractReceivePipeline<StockChannel>
{
    public override string RequestKey => "subscribe";
    
    private readonly ISubscriptionService _subscriptions;
    
    public SubscribePipeline(
        ILoggerFactory loggerFactory,
        ISubscriptionService subscriptions) 
        : base(loggerFactory)
    {
        _subscriptions = subscriptions;
    }
    
    public async Task InvokeAsync(ReceiveContext context, ReceivePipelineDelegate next)
    {
        try
        {
            // Parse request
            var request = context.Request.Form.GetValue<SubscribeRequest>("data");
            
            // Validate
            if (request == null || string.IsNullOrEmpty(request.Symbol))
            {
                context.Response.StatusCode = 400;
                context.Response.Content.Add("error", "Invalid request");
                return;
            }
            
            // Process
            var subscription = await _subscriptions.SubscribeAsync(
                context.Request.ConnectionInfo,
                request
            );
            
            // Respond
            context.Response.StatusCode = 200;
            context.Response.Content.Add("subscription_id", subscription.SubscriptionId);
            
            Logger.LogInformation(
                "Created subscription {SubscriptionId} for {Symbol}",
                subscription.SubscriptionId,
                request.Symbol
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Subscription failed");
            context.Response.StatusCode = 500;
            context.Response.Content.Add("error", ex.Message);
        }
    }
}
```

### Pipeline Chaining

```csharp
// Infrastructure registers pipelines in order
services.AddSingleton<IReceivePipeline, AuthenticationPipeline>();
services.AddSingleton<IReceivePipeline, AuthorizationPipeline>();
services.AddSingleton<IReceivePipeline, ValidationPipeline>();
services.AddSingleton<IReceivePipeline, SubscribePipeline>();

// Execution flow:
// Request → AuthenticationPipeline → AuthorizationPipeline → ValidationPipeline → SubscribePipeline
```

---

**Navigation:**  
[⬆️ Pipelines](../README.md) | [Attributes](Attributes/README.md) | [⬆️ Application Layer](../../README.md)

---

**Statistics:** 4 public types · 4 files · 52 LOC · 1 submodule
