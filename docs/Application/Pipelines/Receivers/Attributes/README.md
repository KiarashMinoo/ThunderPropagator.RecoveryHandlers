# Attributes

> Pipeline documentation attributes

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Attributes for documenting receive pipeline request/response schemas and exception handling, enabling automatic API documentation generation.

**Key Features:**
- 📋 Request schema documentation
- 📤 Response schema documentation
- ⚠️ Exception mapping
- 📖 Automatic API docs generation

## Public Types

### ReceivePipelineRequestSchemaAttribute

**Kind:** Attribute  
**Namespace:** `ThunderPropagator.Application.Pipelines.Receivers.Attributes`  
**Inherits:** `Attribute`

Documents the expected request schema for a pipeline.

**Key Members:**
- `Type RequestType` - Request DTO type

**Usage Recipe:**
```csharp
[ReceivePipelineRequestSchema(typeof(SubscribeRequest))]
public class SubscribePipeline : AbstractReceivePipeline<MyChannel>
{
    // Pipeline implementation
}

public class SubscribeRequest
{
    public string Symbol { get; set; }
    public string[] Fields { get; set; }
}
```

### ReceivePipelineResponseSchemaAttribute

**Kind:** Attribute  
**Namespace:** `ThunderPropagator.Application.Pipelines.Receivers.Attributes`  
**Inherits:** `Attribute`

Documents the response schema for a pipeline.

**Key Members:**
- `Type ResponseType` - Response DTO type

**Usage Recipe:**
```csharp
[ReceivePipelineResponseSchema(typeof(SubscribeResponse))]
public class SubscribePipeline : AbstractReceivePipeline<MyChannel>
{
    // Pipeline implementation
}

public class SubscribeResponse
{
    public string SubscriptionId { get; set; }
    public bool Success { get; set; }
}
```

### ReceivePipelineExceptionAttribute

**Kind:** Attribute  
**Namespace:** `ThunderPropagator.Application.Pipelines.Receivers.Attributes`  
**Inherits:** `Attribute`

Documents possible exceptions and their HTTP status codes.

**Key Members:**
- `Type ExceptionType` - Exception type
- `int StatusCode` - HTTP status code

**Usage Recipe:**
```csharp
[ReceivePipelineException(typeof(InvalidSubscriptionException), 400)]
[ReceivePipelineException(typeof(UnauthorizedException), 401)]
[ReceivePipelineException(typeof(ChannelNotFoundException), 404)]
public class SubscribePipeline : AbstractReceivePipeline<MyChannel>
{
    // Pipeline implementation
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [ReceivePipelineRequestSchemaAttribute.cs](../../src/ThunderPropagator.Application/Pipelines/Receivers/Attributes/ReceivePipelineRequestSchemaAttribute.cs) | 17 | Request schema attribute |
| [ReceivePipelineResponseSchemaAttribute.cs](../../src/ThunderPropagator.Application/Pipelines/Receivers/Attributes/ReceivePipelineResponseSchemaAttribute.cs) | 17 | Response schema attribute |
| [ReceivePipelineExceptionAttribute.cs](../../src/ThunderPropagator.Application/Pipelines/Receivers/Attributes/ReceivePipelineExceptionAttribute.cs) | 21 | Exception mapping attribute |

**Total Files:** 3  
**Total LOC:** 55

## Usage

### Complete Pipeline Documentation

```csharp
/// <summary>
/// Handles stock subscription requests
/// </summary>
[ReceivePipelineRequestSchema(typeof(StockSubscribeRequest))]
[ReceivePipelineResponseSchema(typeof(StockSubscribeResponse))]
[ReceivePipelineException(typeof(InvalidSymbolException), 400)]
[ReceivePipelineException(typeof(UnauthorizedException), 401)]
[ReceivePipelineException(typeof(ChannelNotFoundException), 404)]
[ReceivePipelineException(typeof(RateLimitExceededException), 429)]
public class StockSubscribePipeline : AbstractReceivePipeline<StockChannel>
{
    public override string RequestKey => "subscribe";
    
    // Implementation
}

public class StockSubscribeRequest
{
    /// <summary>Stock symbol to subscribe to</summary>
    public string Symbol { get; set; }
    
    /// <summary>Fields to include in updates</summary>
    public string[] Fields { get; set; }
    
    /// <summary>Subscription mode (Full or Incremental)</summary>
    public SubscriptionMode Mode { get; set; }
}

public class StockSubscribeResponse
{
    /// <summary>Unique subscription identifier</summary>
    public string SubscriptionId { get; set; }
    
    /// <summary>Whether subscription was successful</summary>
    public bool Success { get; set; }
    
    /// <summary>Error message if failed</summary>
    public string? Error { get; set; }
}
```

### Using for API Documentation

```csharp
// Infrastructure can reflect on attributes to generate OpenAPI/Swagger docs
public class ApiDocumentationGenerator
{
    public void GenerateDocs()
    {
        var pipelines = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IReceivePipeline).IsAssignableFrom(t));
            
        foreach (var pipeline in pipelines)
        {
            var requestSchema = pipeline.GetCustomAttribute<ReceivePipelineRequestSchemaAttribute>();
            var responseSchema = pipeline.GetCustomAttribute<ReceivePipelineResponseSchemaAttribute>();
            var exceptions = pipeline.GetCustomAttributes<ReceivePipelineExceptionAttribute>();
            
            // Generate documentation...
        }
    }
}
```

---

**Navigation:**  
[⬆️ Receivers](../README.md) | [⬆️ Pipelines](../../README.md) | [⬆️ Application Layer](../../../README.md)

---

**Statistics:** 3 public types · 3 files · 55 LOC
