# Push Message Formatters

> Pluggable serialization for subscription push notification payloads

## Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Built-in Formatters](#built-in-formatters)
- [Payload Structure](#payload-structure)
- [Configuring the Serializer per Protocol](#configuring-the-serializer-per-protocol)
- [Registering a Custom Formatter](#registering-a-custom-formatter)
- [Runtime Resolution](#runtime-resolution)
- [Files](#files)

## Overview

When ThunderPropagator pushes a `ConnectionSubscriptionPushingMessage` to a connected client, it serializes the payload through an `ISubscriptionMessageFormatter`. The formatter is selected per-protocol via `SerializerType` in the protocol's configuration class, and resolved at send time by `ISubscriptionMessageFormatterRegistry`.

Nine built-in formatters ship out of the box. The system is open for extension: any class implementing `ISubscriptionMessageFormatter` and registered in DI is automatically available.

**Default:** every protocol defaults to `SerializerType.NJson` (Newtonsoft.Json). Existing deployments are unaffected unless `SerializerType` is explicitly changed.

## Architecture

```mermaid
graph TB
    subgraph "Send Path"
        Handler[AbstractConnectionHandler]
        Config[IPushMessageConfiguration<br/>SerializerType]
        Registry[ISubscriptionMessageFormatterRegistry]
        Formatter[ISubscriptionMessageFormatter]
        Bytes[ReadOnlyMemory&lt;byte&gt;]

        Handler -->|reads| Config
        Handler -->|Resolve(serializerType)| Registry
        Registry -->|returns| Formatter
        Formatter -->|Format(message)| Bytes
        Handler -->|sends| Bytes
    end

    subgraph "Built-in Formatters"
        NJson[NJsonSubscriptionMessageFormatter<br/>SerializerType.NJson]
        Json[JsonSubscriptionMessageFormatter<br/>SerializerType.Json]
        NetJson[NetJsonSubscriptionMessageFormatter<br/>SerializerType.NetJson]
        MsgPack[MessagePackSubscriptionMessageFormatter<br/>SerializerType.MessagePack]
        Protobuf[ProtobufSubscriptionMessageFormatter<br/>SerializerType.Protobuf]
        Xml[XmlSubscriptionMessageFormatter<br/>SerializerType.Xml]
        Yaml[YamlSubscriptionMessageFormatter<br/>SerializerType.Yaml]
        Toon[ToonSubscriptionMessageFormatter<br/>SerializerType.Toon]
        Proprietary[ProprietarySubscriptionMessageFormatter<br/>SerializerType.Proprietary]
    end

    subgraph "Extension Point"
        Custom[MyCustomFormatter<br/>implements ISubscriptionMessageFormatter]
    end

    Registry -->|resolves from| NJson
    Registry -->|resolves from| Json
    Registry -->|resolves from| MsgPack
    Registry -->|resolves from| Custom

    style Handler fill:#e1f5ff
    style Registry fill:#fff4e1
    style Custom fill:#e8f5e9
```

## Built-in Formatters

| `SerializerType` | Class | `ContentType` | Notes |
|---|---|---|---|
| `NJson` | `NJsonSubscriptionMessageFormatter` | `application/json` | **Default for all protocols.** Uses Newtonsoft.Json. |
| `Json` | `JsonSubscriptionMessageFormatter` | `application/json` | System.Text.Json via `IFormatSerializerRegistry`. |
| `NetJson` | `NetJsonSubscriptionMessageFormatter` | `application/json` | NetJSON library. |
| `MessagePack` | `MessagePackSubscriptionMessageFormatter` | `application/x-msgpack` | Binary; smallest payload size. |
| `Protobuf` | `ProtobufSubscriptionMessageFormatter` | `application/x-protobuf` | Protocol Buffers. |
| `Xml` | `XmlSubscriptionMessageFormatter` | `application/xml` | XML with `[DataContract]` annotations. |
| `Yaml` | `YamlSubscriptionMessageFormatter` | `application/yaml` | YAML text format. |
| `Toon` | `ToonSubscriptionMessageFormatter` | `application/x-toon` | Toon binary format. |
| `Proprietary` | `ProprietarySubscriptionMessageFormatter` | `application/x-proprietary` | Legacy comma-separated wire format; does not extend `StructuredSubscriptionMessageFormatter`. |

All formatters except `Proprietary` extend `StructuredSubscriptionMessageFormatter`, which builds a `SubscriptionMessagePayload` and delegates to `IFormatSerializerRegistry` for the actual byte encoding. `Proprietary` writes a custom comma-delimited frame directly.

## Payload Structure

All structured formatters (`StructuredSubscriptionMessageFormatter` subclasses) produce a `SubscriptionMessagePayload`:

```
SubscriptionMessagePayload
├── ChannelName          string       Channel that emitted the message
├── Header
│   ├── RequestId        string       Client's subscription request ID
│   ├── FromSnapshot     bool         true when replaying historical state
│   └── RecordStatus     string       "Neutral" | "Added" | "Modified" | "Deleted"
├── Keys
│   └── SubscriptionId   string       Unique subscription identifier
├── NodeRowIds           []           Subscribed key index→value pairs (nullable)
│   ├── Index            int
│   └── Value            string
└── Values               []           Field index→value pairs
    ├── Index            int
    └── Value            string
```

The proprietary format encodes the same logical fields as:
```
{requestId},{fromSnapshot},{recordStatus},{subscriptionId},[{keyIdx}={keyVal};...],{fieldIdx}={fieldVal};...
```

## Configuring the Serializer per Protocol

Set `SerializerType` in the relevant protocol configuration section in `appsettings.json`:

```json
{
  "WebSocket": {
    "SerializerType": "MessagePack"
  },
  "Mqtt": {
    "SerializerType": "Protobuf"
  },
  "Quic": {
    "SerializerType": "Json"
  },
  "InfiniteDataStream": {
    "SerializerType": "NJson"
  },
  "WebTransport": {
    "SerializerType": "Xml"
  }
}
```

Each protocol configuration class (`WebSocketConfiguration`, `MqttConnectionConfiguration`, `QuicConnectionConfiguration`, `InfiniteDataStreamConfiguration`, `WebTransportConfiguration`) reads `SerializerType` and defaults to `SerializerType.NJson` if the key is absent, preserving backward compatibility.

## Registering a Custom Formatter

### Step 1 — Implement `ISubscriptionMessageFormatter`

The simplest path is to extend `StructuredSubscriptionMessageFormatter`, which handles payload building and activity tracing:

```csharp
using ThunderPropagator.Application.PushModules.Formatters;
using ThunderPropagator.BuildingBlocks.Application.Serializations;

public sealed class CborSubscriptionMessageFormatter(IFormatSerializerRegistry registry)
    : StructuredSubscriptionMessageFormatter(registry)
{
    public override SerializerType SerializerType => SerializerType.Cbor; // add to enum if needed
    public override string ContentType => "application/cbor";
}
```

For a fully custom wire format (like `ProprietarySubscriptionMessageFormatter`), implement `ISubscriptionMessageFormatter` directly:

```csharp
public sealed class MyCustomFormatter : ISubscriptionMessageFormatter
{
    public int Order { get; set; } = 0;
    public SerializerType SerializerType => SerializerType.Custom; // your enum value
    public string ContentType => "application/x-custom";

    public IReadOnlyList<ReadOnlyMemory<byte>> Format(
        ConnectionSubscriptionPushingMessage message,
        long maxPushSize = 0)
    {
        // Encode message.FeederMessageFields into your wire format
        var bytes = MyEncoder.Encode(message);
        return [bytes];
    }
}
```

### Step 2 — Register in DI

Add the formatter to the DI container **after** calling `AddThunderPropagator`. Using `TryAddEnumerable` prevents duplicate registrations:

```csharp
builder.Services.AddThunderPropagator(builder.Configuration.GetSection("ThunderPropagator"));

// Register custom formatter
builder.Services.TryAddEnumerable(
    ServiceDescriptor.Singleton<ISubscriptionMessageFormatter, MyCustomFormatter>());
```

### Step 3 — Select via configuration

```json
{
  "WebSocket": {
    "SerializerType": "Custom"
  }
}
```

Or set it programmatically by providing a custom implementation of the protocol's configuration class that overrides `SerializerType`.

## Runtime Resolution

`SubscriptionMessageFormatterRegistry` is built once at application startup from the DI-registered `ISubscriptionMessageFormatter` collection. It maintains two indexes:

| Index | Key | Used by |
|---|---|---|
| `_byType` | `SerializerType` enum value | `Resolve(SerializerType)` — called by `AbstractConnectionHandler` |
| `_byContentType` | `ContentType` string (case-insensitive) | `Resolve(string)` — content-negotiation use cases |

If `SerializerType` is set to a value with no registered formatter, `Resolve` throws `InvalidOperationException` at the first push attempt. Register all required formatters at startup to fail fast during initialization rather than at runtime.

When multiple formatters share the same `ContentType`, `Resolve(string contentType)` returns the one with the lowest `Order` value.

## Files

| File | Purpose |
|---|---|
| [`ISubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/ISubscriptionMessageFormatter.cs) | Formatter contract |
| [`ISubscriptionMessageFormatterRegistry.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/ISubscriptionMessageFormatterRegistry.cs) | Registry contract (resolve by type or content type) |
| [`SubscriptionMessageFormatterRegistry.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/SubscriptionMessageFormatterRegistry.cs) | Default registry implementation |
| [`StructuredSubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/StructuredSubscriptionMessageFormatter.cs) | Base class for structured formatters |
| [`SubscriptionMessagePayload.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/SubscriptionMessagePayload.cs) | Payload DTOs with `[DataContract]` annotations |
| [`JsonSubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/JsonSubscriptionMessageFormatter.cs) | System.Text.Json |
| [`NJsonSubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/NJsonSubscriptionMessageFormatter.cs) | Newtonsoft.Json (default) |
| [`NetJsonSubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/NetJsonSubscriptionMessageFormatter.cs) | NetJSON |
| [`MessagePackSubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/MessagePackSubscriptionMessageFormatter.cs) | MessagePack binary |
| [`ProtobufSubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/ProtobufSubscriptionMessageFormatter.cs) | Protocol Buffers |
| [`XmlSubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/XmlSubscriptionMessageFormatter.cs) | XML |
| [`YamlSubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/YamlSubscriptionMessageFormatter.cs) | YAML |
| [`ToonSubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/ToonSubscriptionMessageFormatter.cs) | Toon binary |
| [`ProprietarySubscriptionMessageFormatter.cs`](../../../../src/ThunderPropagator.Application/PushModules/Formatters/ProprietarySubscriptionMessageFormatter.cs) | Legacy comma-delimited wire format |

**Total Files:** 14

---

**Navigation:**  
🏠 [Documentation Home](../../../README.md) | 📦 [Application Layer](../../README.md)
