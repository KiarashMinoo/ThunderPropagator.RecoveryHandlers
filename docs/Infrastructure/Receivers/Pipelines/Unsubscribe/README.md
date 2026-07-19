# Unsubscribe Pipeline

## Overview
Handles subscription removal, supporting targeted unsubscription by subscription IDs or complete connection unsubscription.

## Files (4 files, 220 LOC)

| File | LOC | Description |
|------|-----|-------------|
| UnsubscribeChannelReceivePipeline.cs | 83 | Main unsubscribe pipeline |
| UnsubscribeChannelReceivePipelineRequestDto.cs | 80 | Request DTO with JSON converter |
| UnsubscribeChannelReceivePipelineResponseDto.cs | 13 | Response DTO |
| SubscribedKeysCollection.cs | 24 | Subscribed keys collection |
| SubscriptionIdsRequiredException.cs | 20 | Missing subscription IDs exception |

## Unsubscribe Modes

### 1. Targeted Unsubscription (by Subscription IDs)

```json
{
  "RequestKey": "Unsubscribe",
  "SubscribedKeys": {
    "sub-1": {},
    "sub-3": {}
  }
}
```

**Behavior**: Removes only specified subscriptions (`sub-1`, `sub-3`). Other subscriptions remain active.

### 2. Complete Unsubscription (all subscriptions)

```json
{
  "RequestKey": "Unsubscribe"
}
```

**Behavior**: Removes ALL subscriptions for the connection.

## Examples

### Unsubscribe Specific Symbols

```csharp
// Previously subscribed to AAPL, GOOGL, MSFT
// Now unsubscribe from GOOGL and MSFT

var request = new
{
    RequestKey = "Unsubscribe",
    SubscribedKeys = new Dictionary<string, object>
    {
        ["stocks-googl"] = new { },
        ["stocks-msft"] = new { }
    }
};

await _webSocket.SendAsync(JsonSerializer.Serialize(request));

// AAPL subscription remains active
```

### Unsubscribe All

```csharp
var request = new { RequestKey = "Unsubscribe" };
await _webSocket.SendAsync(JsonSerializer.Serialize(request));

// All subscriptions removed
```

## See Also
- [Subscribe Pipeline](../Subscribe/README.md)
- [Subscription Management](../../../../../Application/Channels/Subscribers/README.md)
