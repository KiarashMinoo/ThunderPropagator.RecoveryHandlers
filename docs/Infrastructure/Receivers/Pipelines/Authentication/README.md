# Authentication Pipeline

## Table of Contents
- [Overview](#overview)
- [Files](#files)
- [Types](#types)
- [Type Details](#type-details)
- [Authentication Flow](#authentication-flow)
- [Examples](#examples)
- [See Also](#see-also)

## Overview

The Authentication Pipeline validates user credentials before processing requests. It supports Basic authentication (username/password with RSA encryption) and OAuth2 authentication (JWT tokens). The pipeline uses a handler-based architecture allowing multiple authentication strategies to coexist and execute based on channel configuration.

## Files

| File | LOC | Description |
|------|-----|-------------|
| [AuthenticationReceivePipeline.cs](../../../../src/ThunderPropagator.Infrastructure/Receivers/Pipelines/Authentication/AuthenticationReceivePipeline.cs) | 65 | Main authentication pipeline orchestrator |
| [AuthenticationReceivePipelineHandlerInvoker.cs](../../../../src/ThunderPropagator.Infrastructure/Receivers/Pipelines/Authentication/AuthenticationReceivePipelineHandlerInvoker.cs) | 70 | Reflection-based handler invoker |
| [AuthenticationReceivePipelinesManager.cs](../../../../src/ThunderPropagator.Infrastructure/Receivers/Pipelines/Authentication/AuthenticationReceivePipelinesManager.cs) | 33 | Handler discovery and invoker builder |
| **AuthenticationReceivePipelineHandlers/** | | |
| [IAuthenticationReceivePipelineHandler.cs](../../../../src/ThunderPropagator.Infrastructure/Receivers/Pipelines/Authentication/AuthenticationReceivePipelineHandlers/IAuthenticationReceivePipelineHandler.cs) | 13 | Base handler interface |
| [BasicAuthenticationReceivePipelineHandler.cs](../../../../src/ThunderPropagator.Infrastructure/Receivers/Pipelines/Authentication/AuthenticationReceivePipelineHandlers/BasicAuthenticationReceivePipelineHandler.cs) | 92 | Basic auth with RSA decryption |
| [OAuth2AuthenticationReceivePipelineHandler.cs](../../../../src/ThunderPropagator.Infrastructure/Receivers/Pipelines/Authentication/AuthenticationReceivePipelineHandlers/OAuth2AuthenticationReceivePipelineHandler.cs) | 56 | JWT token validation |
| **Exceptions/** | | |
| [UnauthenticatedException.cs](../../../../src/ThunderPropagator.Infrastructure/Receivers/Pipelines/Authentication/Exceptions/UnauthenticatedException.cs) | 17 | General authentication failure |
| [UnauthenticatedTokenException.cs](../../../../src/ThunderPropagator.Infrastructure/Receivers/Pipelines/Authentication/Exceptions/UnauthenticatedTokenException.cs) | 15 | Invalid JWT token |
| [UnauthenticatedUserException.cs](../../../../src/ThunderPropagator.Infrastructure/Receivers/Pipelines/Authentication/Exceptions/UnauthenticatedUserException.cs) | 15 | Invalid credentials |

**Total**: 11 files, 428 LOC

## Types

| Type | Kind | Summary | Implements/Inherits |
|------|------|---------|---------------------|
| `AuthenticationReceivePipeline<TChannel>` | Class | Main authentication pipeline | `AbstractReceivePipeline<TChannel>` |
| `AuthenticationReceivePipelineHandlerInvoker` | Class | Invokes handlers via reflection | - |
| `AuthenticationReceivePipelinesManager<TChannel>` | Class | Manages handler discovery and invoker creation | - |
| `IAuthenticationReceivePipelineHandler` | Interface | Base handler interface | - |
| `IAuthenticationReceivePipelineHandler<TChannel>` | Interface | Channel-specific handler interface | `IAuthenticationReceivePipelineHandler` |
| `BasicAuthenticationReceivePipelineHandler<TChannel>` | Class | Basic authentication with RSA | `IAuthenticationReceivePipelineHandler<TChannel>` |
| `OAuth2AuthenticationReceivePipelineHandler<TChannel>` | Class | OAuth2 JWT validation | `IAuthenticationReceivePipelineHandler<TChannel>` |
| `UnauthenticatedException` | Exception | General authentication failure | `HttpRequestException` |
| `UnauthenticatedTokenException` | Exception | Invalid JWT token | `HttpRequestException` |
| `UnauthenticatedUserException` | Exception | Invalid username/password | `HttpRequestException` |

## Type Details

### AuthenticationReceivePipeline<TChannel>

**Purpose**: Orchestrates authentication handler execution for incoming requests.

**Key Properties**:
- `RequestKey`: Returns `RequestContextHelper.NoRequestKey` (executes for all requests)
- `_counter`: Telemetry counter for authentication attempts

**Processing Logic**:
1. Check `channelInfo.Channel.Metadata.Authentication.IsEnabled`
2. Get authentication handlers from manager
3. Build handler invokers
4. Execute each invoker sequentially
5. Record telemetry metrics
6. Continue to next pipeline via `next.Invoke()`

### BasicAuthenticationReceivePipelineHandler<TChannel>

**Purpose**: Validates username/password credentials with RSA decryption.

**Authentication Flow**:
1. Extract `Username` and `Password` from request
2. Decrypt using RSA private key from channel metadata
3. Lookup user in `BasicUsersConfiguration`
4. Create `ClaimsPrincipal` with roles
5. Inject identity into request context

**RSA Configuration**:
```csharp
Metadata.Authentication.EncryptionPrivateKey = "-----BEGIN RSA PRIVATE KEY-----...";
Metadata.Authentication.EncryptionKeySize = 2048;
```

### OAuth2AuthenticationReceivePipelineHandler<TChannel>

**Purpose**: Validates JWT tokens using channel JWT configuration.

**Validation Process**:
1. Extract `Token` from request
2. Validate JWT using `JwtIdentityHelper.IsTokenValid()`
3. Check token signature, expiration, issuer, audience
4. Extract `ClaimsPrincipal` from token
5. Inject identity into request context

**JWT Configuration**:
```csharp
Metadata.Authentication.JwtConfiguration = new ChannelJwtConfiguration
{
    SecretKey = "your-secret-key",
    Issuer = "https://your-issuer.com",
    Audience = "your-api",
    ExpirationMinutes = 60
};
```

## Authentication Flow

```mermaid
sequenceDiagram
    participant Client
    participant Pipeline as AuthenticationReceivePipeline
    participant Manager as AuthenticationReceivePipelinesManager
    participant BasicHandler as BasicAuthenticationHandler
    participant OAuth2Handler as OAuth2AuthenticationHandler
    participant Next as Next Pipeline

    Client->>Pipeline: Request with credentials
    
    alt Authentication Enabled
        Pipeline->>Manager: GetAuthenticationReceivePipelineHandlers()
        Manager-->>Pipeline: [BasicHandler, OAuth2Handler]
        
        Pipeline->>Manager: BuildAuthenticationReceivePipelineHandlerInvokers()
        Manager-->>Pipeline: [Invoker1, Invoker2]
        
        alt Basic Authentication
            Pipeline->>BasicHandler: Invoke(context)
            BasicHandler->>BasicHandler: Decrypt username/password (RSA)
            BasicHandler->>BasicHandler: Lookup user in BasicUsersConfiguration
            
            alt Valid Credentials
                BasicHandler->>BasicHandler: Create ClaimsPrincipal
                BasicHandler->>Context: Inject identity
                BasicHandler-->>Pipeline: Success
            else Invalid Credentials
                BasicHandler-->>Client: 401 UnauthenticatedUserException
            end
        end
        
        alt OAuth2 Authentication
            Pipeline->>OAuth2Handler: Invoke(context)
            OAuth2Handler->>OAuth2Handler: Validate JWT token
            
            alt Valid Token
                OAuth2Handler->>OAuth2Handler: Extract claims
                OAuth2Handler->>Context: Inject identity
                OAuth2Handler-->>Pipeline: Success
            else Invalid Token
                OAuth2Handler-->>Client: 401 UnauthenticatedTokenException
            end
        end
        
        Pipeline->>Pipeline: Record metrics
    end
    
    Pipeline->>Next: next.Invoke(context)
```

## Examples

### Basic Authentication Configuration

```csharp
public class StockChannelConfiguration : AbstractChannelConfiguration
{
    public StockChannelConfiguration()
    {
        Metadata.Authentication.IsEnabled = true;
        Metadata.Authentication.AuthenticationType = AuthenticationType.Basic;
        Metadata.Authentication.EncryptionPrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEA0Z3VS...your_private_key...aBcDeFgH
-----END RSA PRIVATE KEY-----";
        Metadata.Authentication.EncryptionKeySize = 2048;
        
        Metadata.Authentication.BasicUsersConfiguration = new[]
        {
            new BasicUserConfiguration
            {
                Username = "admin",
                Password = "AdminPassword123!",
                Roles = new[] { "Admin", "Trader", "Viewer" }
            },
            new BasicUserConfiguration
            {
                Username = "trader",
                Password = "TraderPassword456!",
                Roles = new[] { "Trader", "Viewer" }
            },
            new BasicUserConfiguration
            {
                Username = "viewer",
                Password = "ViewerPassword789!",
                Roles = new[] { "Viewer" }
            }
        };
    }
}
```

### Client-Side RSA Encryption (C#)

```csharp
public class AuthenticationClient
{
    private readonly RSA _rsa;
    
    public AuthenticationClient(string publicKey)
    {
        _rsa = RSA.Create();
        _rsa.ImportFromPem(publicKey);
    }
    
    public async Task<string> SubscribeAsync(string username, string password)
    {
        // Encrypt credentials
        var encryptedUsername = Convert.ToBase64String(
            _rsa.Encrypt(Encoding.UTF8.GetBytes(username), RSAEncryptionPadding.OaepSHA256));
        var encryptedPassword = Convert.ToBase64String(
            _rsa.Encrypt(Encoding.UTF8.GetBytes(password), RSAEncryptionPadding.OaepSHA256));
        
        var request = new
        {
            RequestKey = "Subscribe",
            Username = encryptedUsername,
            Password = encryptedPassword,
            SubscribingKeys = new Dictionary<string, object>
            {
                ["sub-1"] = new { Symbol = "AAPL" }
            },
            SubscribingFields = new[] { "LastPrice", "Volume" }
        };
        
        await _webSocket.SendAsync(JsonSerializer.Serialize(request));
        return await _webSocket.ReceiveAsync();
    }
}
```

### OAuth2 Authentication Configuration

```csharp
public class StockChannelConfiguration : AbstractChannelConfiguration
{
    public StockChannelConfiguration()
    {
        Metadata.Authentication.IsEnabled = true;
        Metadata.Authentication.AuthenticationType = AuthenticationType.OAuth2;
        
        Metadata.Authentication.JwtConfiguration = new ChannelJwtConfiguration
        {
            SecretKey = "your-256-bit-secret-key-here-must-be-long-enough",
            Issuer = "https://auth.stockmarket.com",
            Audience = "stock-channel-api",
            ExpirationMinutes = 60,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    }
}
```

### Client-Side JWT Authentication

```csharp
public class JwtAuthenticationClient
{
    private string? _jwtToken;
    
    public async Task<string> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { username, password });
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _jwtToken = result.Token;
        return _jwtToken;
    }
    
    public async Task SubscribeAsync()
    {
        var request = new
        {
            RequestKey = "Subscribe",
            Token = _jwtToken,
            SubscribingKeys = new Dictionary<string, object>
            {
                ["sub-1"] = new { Symbol = "AAPL" }
            },
            SubscribingFields = new[] { "LastPrice", "Volume" }
        };
        
        await _webSocket.SendAsync(JsonSerializer.Serialize(request));
    }
}
```

### Custom Authentication Handler

```csharp
public class ApiKeyAuthenticationHandler<TChannel> : IAuthenticationReceivePipelineHandler<TChannel>
    where TChannel : class, IChannel
{
    private readonly TChannel _channel;
    private readonly IApiKeyValidator _apiKeyValidator;
    
    public async Task Invoke(ClientRequestContext requestContext, CancellationToken cancellationToken = default)
    {
        if (_channel.Metadata.Authentication.AuthenticationType == AuthenticationType.ApiKey)
        {
            if (!requestContext.RequestContentForm.TryGetValue("ApiKey", out var apiKeyObj))
                throw new UnauthenticatedException("API key is required");
            
            var apiKey = apiKeyObj.ToString();
            var user = await _apiKeyValidator.ValidateAsync(apiKey, cancellationToken);
            
            if (user == null)
                throw new UnauthenticatedUserException();
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.Username)
            };
            
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
            
            var identity = new ClaimsIdentity(claims, "ApiKey");
            var principal = new ClaimsPrincipal(identity);
            
            requestContext.InjectRequestContentFormData(nameof(RequestContext.Identity), principal);
        }
    }
}

// Register custom handler
services.TryAddSingleton<IAuthenticationReceivePipelineHandler<StockChannel>, ApiKeyAuthenticationHandler<StockChannel>>();
```

## See Also

- [Authorization Pipeline](../Authorization/README.md) - Next pipeline in chain
- [Application Authentication](../../../../Application/Channels/Metadata/README.md#authentication-metadata) - Authentication configuration
- [BuildingBlocks Ciphering](https://github.com/KiarashMinoo/ThunderPropagator) - RSA encryption utilities
- [JWT Helper](../../../../Application/Helpers/README.md) - JWT validation helpers
- [Pipelines Overview](../README.md) - Complete pipeline catalog
