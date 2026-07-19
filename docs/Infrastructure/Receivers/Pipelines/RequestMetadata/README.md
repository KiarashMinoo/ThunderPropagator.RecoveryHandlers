# RequestMetadata Pipeline

## Overview
Returns channel metadata including authentication requirements, authorization configuration, and field descriptors.

## Files (2 files, 78 LOC)

| File | LOC | Description |
|------|-----|-------------|
| RequestMetadataReceivePipeline.cs | 64 | Metadata request pipeline |
| RequestMetadataReceivePipelineResponseDto.cs | 14 | Response DTO with metadata |

## Request/Response

**Request**:
```json
{
  "RequestKey": "RequestMetadata"
}
```

**Response**:
```json
{
  "Metadata": {
    "ChannelName": "StockChannel",
    "ChannelType": "RealTimeStock",
    "Authentication": {
      "IsEnabled": true,
      "AuthenticationType": "Basic"
    },
    "Authorization": {
      "IsEnabled": true,
      "Roles": ["Trader", "Admin"]
    },
    "Fields": [
      {
        "Name": "Symbol",
        "Type": "string",
        "Required": true,
        "Description": "Stock symbol"
      },
      {
        "Name": "LastPrice",
        "Type": "decimal",
        "Required": true,
        "Description": "Current price"
      }
    ]
  }
}
```

## Use Cases

1. **Client Initialization**: Discover channel capabilities before subscribing
2. **Dynamic UI Generation**: Build subscription forms based on available fields
3. **Validation**: Verify subscription requests match channel schema
4. **Documentation**: Auto-generate API documentation

## See Also
- [Channel Metadata](../../../../../Application/Channels/Metadata/README.md)
