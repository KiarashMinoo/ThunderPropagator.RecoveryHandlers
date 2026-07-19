# Infrastructure.Exceptions

## Overview
The Infrastructure.Exceptions namespace contains HTTP-based exceptions specific to protocol handling and client validation.

## Contents
- [Public Types](#public-types)
- [Files](#files)
- [Usage Examples](#usage-examples)
- [Related Documentation](#related-documentation)

## Public Types

### InvalidClientException
**Kind**: Public sealed exception (non-sealed in DEBUG builds)  
**Inherits**: `HttpRequestException`  
**Summary**: Thrown when client validation fails (e.g., connection info invalid, authentication failure).

**Constructor**:
```csharp
public InvalidClientException(string message, Exception? inner = null)
    : base(message, inner, HttpStatusCode.BadRequest)
```

**HTTP Status**: 400 Bad Request

**Usage Recipe**:
```csharp
public async Task<IConnectionInfo> ValidateClientAsync(HttpContext httpContext)
{
    var clientId = httpContext.Request.Headers["X-Client-Id"].FirstOrDefault();
    
    if (string.IsNullOrEmpty(clientId))
    {
        throw new InvalidClientException("Missing X-Client-Id header");
    }
    
    var isValid = await _clientValidator.ValidateAsync(clientId);
    if (!isValid)
    {
        throw new InvalidClientException($"Invalid client ID: {clientId}");
    }
    
    return new WebSocketConnectionInfo(httpContext);
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [InvalidClientException.cs](../../src/ThunderPropagator.Infrastructure/Exceptions/InvalidClientException.cs) | 19 | Client validation exception |

**Total**: 1 file, 19 LOC

## Usage Examples

### Protocol Handler Validation
```csharp
protected override async Task OnConnectAsync(WebSocketConnectionInfo connectionInfo)
{
    if (!connectionInfo.IsAvailable)
    {
        throw new InvalidClientException(
            "Connection is not available",
            new InvalidOperationException("WebSocket not open"));
    }
    
    await base.OnConnectAsync(connectionInfo);
}
```

### Middleware Exception Handling
```csharp
public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
{
    try
    {
        await next(httpContext);
    }
    catch (InvalidClientException ex)
    {
        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            Error = ex.Message,
            StatusCode = 400
        });
    }
}
```

## Related Documentation
- [Parent: Infrastructure Layer](../README.md)
- [Application: Exceptions](../../Application/Channels/Exceptions/README.md)
- [Infrastructure: Protocols](../Protocols/README.md)

---

**Namespace**: `ThunderPropagator.Infrastructure.Exceptions`  
**Types**: 1 public type  
**Files**: 1 file (19 LOC)  
**Diagrams**: ✗  
**Last Updated**: December 28, 2025
