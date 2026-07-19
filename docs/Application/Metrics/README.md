# Metrics

> Telemetry and metrics for monitoring system performance

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

OpenTelemetry-based metrics and telemetry for monitoring feeder states, message throughput, and system performance.

**Key Features:**
- 📊 Feeder state metrics
- 📈 Message throughput counters
- 🔍 OpenTelemetry integration
- 📉 Histograms for message sizes

## Public Types

### FeedersTelemetry

**Kind:** Static Class  
**Namespace:** `ThunderPropagator.Application.Metrics`

Tracks feeder lifecycle state changes.

**Key Members:**
- `static Counter<int> FeedersCounter` - Feeder state counter
- `static void RecordFeederState(FeederState state, string channelName)` - Record state

**Usage Recipe:**
```csharp
// Automatically called by AbstractFeeder
FeedersTelemetry.RecordFeederState(FeederState.Started, "stocks");
```

### PushedMessageTelemetry

**Kind:** Static Class  
**Namespace:** `ThunderPropagator.Application.Metrics`

Tracks outbound message metrics.

**Key Members:**
- `static Counter<long> MessageCount` - Message count counter
- `static Histogram<long> MessageSize` - Message size histogram

**Usage Recipe:**
```csharp
// Record message push
PushedMessageTelemetry.MessageCount.Add(1, new("channel", "stocks"));
PushedMessageTelemetry.MessageSize.Record(1024, new("channel", "stocks"));
```

### ReceivedMessageTelemetry

**Kind:** Static Class  
**Namespace:** `ThunderPropagator.Application.Metrics`

Tracks inbound message metrics.

**Key Members:**
- `static Counter<long> MessageCount` - Received message counter
- `static Histogram<long> MessageSize` - Received message size histogram

**Usage Recipe:**
```csharp
// Record message reception
ReceivedMessageTelemetry.MessageCount.Add(1, new("protocol", "websocket"));
ReceivedMessageTelemetry.MessageSize.Record(512, new("protocol", "websocket"));
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [FeedersTelemetry.cs](../../src/ThunderPropagator.Application/Metrics/FeedersTelemetry.cs) | 19 | Feeder state metrics |
| [PushedMessageTelemetry.cs](../../src/ThunderPropagator.Application/Metrics/PushedMessageTelemetry.cs) | 17 | Outbound message metrics |
| [ReceivedMessageTelemetry.cs](../../src/ThunderPropagator.Application/Metrics/ReceivedMessageTelemetry.cs) | 21 | Inbound message metrics |

**Total Files:** 3  
**Total LOC:** 57

## Usage

### Monitoring Metrics

```csharp
// Metrics are automatically exported via OpenTelemetry
services.AddOpenTelemetry()
    .WithMetrics(builder => builder
        .AddMeter("ThunderPropagator.Application")
        .AddPrometheusExporter()
    );
```

### Custom Metric Reporting

```csharp
public class MyFeeder : AbstractFeeder<MyChannel, MyMessage, MyConfig>
{
    protected override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Metrics automatically recorded by base class
        await base.StartAsync(cancellationToken);
    }
}
```

---

**Navigation:**  
[⬆️ Application Layer](../README.md)

---

**Statistics:** 3 public types · 3 files · 57 LOC
