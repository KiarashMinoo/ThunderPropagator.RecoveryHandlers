# Subscribe Pipeline

## Overview
Handles subscription creation with key/field filtering and subscription mode configuration. Supports incremental and full subscription modes.

## Files (6 files, 310 LOC)

| File | LOC | Description |
|------|-----|-------------|
| SubscribeReceivePipeline.cs | 89 | Main subscribe pipeline |
| SubscribeReceivePipelineRequestDto.cs | 111 | Request DTO with custom JSON converter |
| SubscribeReceivePipelineResponseDto.cs | 13 | Response DTO |
| SubscribingKeysCollection.cs | 51 | Key collection with JSON converter |
| SubscribingFieldsCollection.cs | 17 | Field collection |
| UnsubscribableAttribute.cs | 11 | Marks channels as non-unsubscribable |
| SubscribingKeysRequiredException.cs | 18 | Missing keys exception |
| SubscribingFieldsRequiredException.cs | 18 | Missing fields exception |
| AtLeastOneSubscribingFieldRequiredException.cs | 18 | At least one field required |

## Request Structure

```typescript
{
  "RequestKey": "Subscribe",
  "SubscribingKeys": {
    "subscription-id-1": {
      "Symbol": "AAPL",
      "Exchange": "NASDAQ"
    },
    "subscription-id-2": {
      "Symbol": "GOOGL",
      "Exchange": "NASDAQ"
    }
  },
  "SubscribingFields": ["LastPrice", "Volume", "Timestamp", "High", "Low"],
  "SubscriptionMode": "Incremental" // or "Full"
}
```

## Subscription Modes

| Mode | Behavior | Use Case |
|------|----------|----------|
| **Full** | Sends all subscribed fields on every update | Real-time dashboards, complete data snapshots |
| **Incremental** | Sends only changed fields since last update | Bandwidth optimization, high-frequency updates |

## Examples

### Full Mode Subscription

```csharp
// Request
{
  "RequestKey": "Subscribe",
  "SubscribingKeys": { "sub-1": { "Symbol": "AAPL" } },
  "SubscribingFields": ["LastPrice", "Volume", "High", "Low"],
  "SubscriptionMode": "Full"
}

// Every update includes ALL fields:
{
  "SubscriptionId": "sub-1",
  "Symbol": "AAPL",
  "LastPrice": 150.25,
  "Volume": 1000000,
  "High": 151.00,
  "Low": 149.50,
  "Timestamp": "2025-12-28T10:30:00Z"
}
```

### Incremental Mode Subscription

```csharp
// Request
{
  "RequestKey": "Subscribe",
  "SubscribingKeys": { "sub-1": { "Symbol": "AAPL" } },
  "SubscribingFields": ["LastPrice", "Volume"],
  "SubscriptionMode": "Incremental"
}

// First update (all fields):
{
  "SubscriptionId": "sub-1",
  "Symbol": "AAPL",
  "LastPrice": 150.25,
  "Volume": 1000000,
  "Timestamp": "2025-12-28T10:30:00Z"
}

// Second update (only LastPrice changed):
{
  "SubscriptionId": "sub-1",
  "Symbol": "AAPL",
  "LastPrice": 150.30,
  "Timestamp": "2025-12-28T10:30:05Z"
}
```

### Multi-Symbol Subscription

```csharp
var request = new
{
    RequestKey = "Subscribe",
    SubscribingKeys = new Dictionary<string, object>
    {
        ["stocks-aapl"] = new { Symbol = "AAPL", Exchange = "NASDAQ" },
        ["stocks-googl"] = new { Symbol = "GOOGL", Exchange = "NASDAQ" },
        ["stocks-msft"] = new { Symbol = "MSFT", Exchange = "NASDAQ" }
    },
    SubscribingFields = new[] { "LastPrice", "Volume", "Timestamp" },
    SubscriptionMode = "Incremental"
};

// Response includes all subscriptions:
{
  "Message": "Subscribed",
  "Subscriptions": [
    { "SubscriptionId": "stocks-aapl", "Keys": { "Symbol": "AAPL", "Exchange": "NASDAQ" } },
    { "SubscriptionId": "stocks-googl", "Keys": { "Symbol": "GOOGL", "Exchange": "NASDAQ" } },
    { "SubscriptionId": "stocks-msft", "Keys": { "Symbol": "MSFT", "Exchange": "NASDAQ" } }
  ]
}
```

## See Also
- [Unsubscribe Pipeline](../Unsubscribe/README.md)
- [Subscription Architecture](../../../../../Application/Channels/Subscribers/README.md)
