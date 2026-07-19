# Contexts

> Request and response context types for pipeline processing

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Context types encapsulate request and response information flowing through receive and push pipelines, providing typed access to connection details, query parameters, form data, and routing information.

**Key Features:**
- 📥 Request context with routing and query strings
- 📤 Response context with status codes
- 🔄 Receive context (request + response pair)
- 📨 Push context for outbound messages

## Public Types

### RequestContext

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Contexts`

Encapsulates incoming request information.

**Key Members:**
- `IConnectionInfo ConnectionInfo` - Connection making request
- `string RequestId` - Unique request identifier
- `string Action` - Request action/command
- `QueryStringsCollection QueryStrings` - Query string parameters
- `RequestContentFormCollection Form` - Form data
- `RouteTableCollection RouteTable` - Route parameters
- `Dictionary<string, object?> Items` - Request-scoped storage

**Usage Recipe:**
```csharp
public class MyPipeline : AbstractReceivePipeline<MyChannel>
{
    public async Task InvokeAsync(ReceiveContext context, ReceivePipelineDelegate next)
    {
        var request = context.Request;
        
        // Access query parameters
        var page = request.QueryStrings.GetValue<int>("page") ?? 1;
        
        // Access form data
        var username = request.Form.GetValue<string>("username");
        
        // Access route parameters
        var id = request.RouteTable.GetValue<int>("id");
        
        // Store in request items
        request.Items["user"] = await GetUserAsync(username);
        
        await next(context);
    }
}
```

### ResponseContext

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Contexts`

Encapsulates outgoing response information.

**Key Members:**
- `int StatusCode` - HTTP-style status code
- `string? StatusMessage` - Status message
- `ResponseContentFormCollection Content` - Response content
- `Dictionary<string, string> Headers` - Response headers

**Usage Recipe:**
```csharp
public async Task InvokeAsync(ReceiveContext context, ReceivePipelineDelegate next)
{
    var response = context.Response;
    
    // Set status
    response.StatusCode = 200;
    response.StatusMessage = "OK";
    
    // Add headers
    response.Headers["X-Custom"] = "value";
    
    // Set content
    response.Content.Add("success", true);
    response.Content.Add("data", new { id = 123 });
}
```

### ReceiveContext

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Contexts`

Combines request and response for receive pipeline processing.

**Key Members:**
- `RequestContext Request` - Request context
- `ResponseContext Response` - Response context

**Usage Recipe:**
```csharp
public async Task InvokeAsync(ReceiveContext context, ReceivePipelineDelegate next)
{
    // Access both request and response
    var action = context.Request.Action;
    context.Response.StatusCode = 200;
    
    await next(context);
}
```

### PushContext

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Contexts`

Context for push pipeline message processing.

**Key Members:**
- `IChannel Channel` - Source channel
- `ReadOnlyMemory<byte> Message` - Message bytes
- `IConnectionInfo ConnectionInfo` - Target connection
- `Dictionary<string, object?> Items` - Context-scoped storage

**Usage Recipe:**
```csharp
public class CompressionPipeline : AbstractPushPipeline<MyChannel>
{
    public async Task InvokeAsync(PushContext context, PushPipelineDelegate next)
    {
        // Compress large messages
        if (context.Message.Length > 1024)
        {
            var compressed = Compress(context.Message);
            context.Items["compressed"] = true;
            // Update message
        }
        
        await next(context);
    }
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [RequestContext.cs](../../src/ThunderPropagator.Application/Channels/Contexts/RequestContext.cs) | 55 | Request context with routing and query strings |
| [ResponseContext.cs](../../src/ThunderPropagator.Application/Channels/Contexts/ResponseContext.cs) | 32 | Response context with status and content |
| [ReceiveContext.cs](../../src/ThunderPropagator.Application/Channels/Contexts/ReceiveContext.cs) | 16 | Request + response container |
| [PushContext.cs](../../src/ThunderPropagator.Application/Channels/Contexts/PushContext.cs) | 21 | Push message context |

**Total Files:** 4  
**Total LOC:** 124

## Usage

### Receive Pipeline with Context

```csharp
public class ValidationPipeline : AbstractReceivePipeline<StockChannel>
{
    public override string RequestKey => "subscribe";
    
    public ValidationPipeline(ILoggerFactory loggerFactory) 
        : base(loggerFactory)
    {
    }
    
    public async Task InvokeAsync(ReceiveContext context, ReceivePipelineDelegate next)
    {
        // Validate request
        var symbol = context.Request.Form.GetValue<string>("symbol");
        if (string.IsNullOrEmpty(symbol))
        {
            context.Response.StatusCode = 400;
            context.Response.StatusMessage = "Symbol required";
            context.Response.Content.Add("error", "Missing symbol parameter");
            return;
        }
        
        // Store validated data
        context.Request.Items["validated_symbol"] = symbol.ToUpper();
        
        // Continue pipeline
        await next(context);
    }
}
```

### Push Pipeline with Context

```csharp
public class EncryptionPipeline : AbstractPushPipeline<SecureChannel>
{
    public EncryptionPipeline(ILoggerFactory loggerFactory) 
        : base(loggerFactory)
    {
    }
    
    public async Task InvokeAsync(PushContext context, PushPipelineDelegate next)
    {
        // Encrypt message
        var encrypted = await EncryptAsync(context.Message);
        context.Items["encrypted"] = true;
        context.Items["original_size"] = context.Message.Length;
        
        // Continue with encrypted message
        await next(context);
    }
}
```

---

**Navigation:**  
[⬆️ Channels](../README.md) | [⬆️ Application Layer](../../README.md)

---

**Statistics:** 4 public types · 4 files · 124 LOC
