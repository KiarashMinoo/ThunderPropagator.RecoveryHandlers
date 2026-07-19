# Metadata

> Channel metadata and configuration types

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Channel metadata defines the properties, security settings, and behavioral characteristics of channels including authentication, authorization, encryption, and snapshot configuration.

**Key Features:**
- 📋 Channel descriptive metadata
- 🔐 Authentication configuration (Basic, JWT)
- 🛡️ Authorization metadata
- 🔒 Message encryption settings
- 💾 Snapshot configuration
- 📊 Program descriptors collection

## Public Types

### IChannelMetadata

**Kind:** Interface  
**Namespace:** `ThunderPropagator.Application.Channels.Metadata`

Core interface defining channel metadata.

**Key Members:**
- `string ChannelName` - Unique channel identifier
- `string ChannelDisplayName` - Human-readable name
- `string ChannelDescription` - Channel description
- `ChannelProgramsDescriptorCollection ChannelProgramsDescriptors` - Field descriptors
- `ChannelAuthenticationMetadata? Authentication` - Authentication config
- `ChannelAuthorizationMetadata? Authorization` - Authorization config
- `ChannelMessageEncryptionMetadata? MessageEncryption` - Encryption config
- `ChannelSnapshotMetadata Snapshot` - Snapshot settings

### AbstractChannelMetadata

**Kind:** Abstract Class  
**Namespace:** `ThunderPropagator.Application.Channels.Metadata`  
**Implements:** `IChannelMetadata`

Base metadata implementation with initialization logic.

**Key Members:**
- All IChannelMetadata properties
- `void Initialize()` - Initialize metadata collections

**Usage Recipe:**
```csharp
public class StockMetadata : AbstractChannelMetadata
{
    public override string ChannelName => "stocks";
    public override string ChannelDisplayName => "Stock Prices";
    public override string ChannelDescription => "Real-time stock price updates";
    
    public StockMetadata()
    {
        // Configure programs
        ChannelProgramsDescriptors.Add(new SubscribingKeyChannelProgramsDescriptor
        {
            Name = "symbol",
            Index = 0,
            DisplayName = "Stock Symbol"
        });
        
        ChannelProgramsDescriptors.Add(new DecimalChannelProgramsDescriptor
        {
            Name = "price",
            Index = 1,
            DisplayName = "Price",
            DecimalPlaces = 2
        });
        
        // Configure authentication
        Authentication = new ChannelAuthenticationMetadata
        {
            IsEnabled = true,
            BasicUserConfiguration = new ChannelBasicUserConfiguration
            {
                Username = "admin",
                Password = "secret"
            }
        };
        
        // Configure snapshot
        Snapshot = new ChannelSnapshotMetadata
        {
            IsEnabled = true,
            IsTimeSeries = false
        };
    }
}
```

### ChannelAuthenticationMetadata

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Metadata`

Authentication configuration for channel access.

**Key Members:**
- `bool IsEnabled` - Authentication enabled
- `ChannelBasicUserConfiguration? BasicUserConfiguration` - Basic auth config
- `ChannelJwtConfiguration? JwtConfiguration` - JWT auth config

**Usage Recipe:**
```csharp
var metadata = new StockMetadata
{
    Authentication = new ChannelAuthenticationMetadata
    {
        IsEnabled = true,
        BasicUserConfiguration = new ChannelBasicUserConfiguration
        {
            Username = "user",
            Password = "pass"
        }
    }
};
```

### ChannelBasicUserConfiguration

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Metadata`

Basic authentication credentials.

**Key Members:**
- `string Username` - Username
- `string Password` - Password

### ChannelJwtConfiguration

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Metadata`

JWT authentication configuration.

**Key Members:**
- `string Issuer` - Token issuer
- `string Audience` - Token audience
- `string SecretKey` - Signing key

### ChannelAuthorizationMetadata

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Metadata`

Authorization rules and policies.

**Key Members:**
- `bool IsEnabled` - Authorization enabled
- `string[] RequiredRoles` - Required user roles
- `string[] RequiredClaims` - Required claims

**Usage Recipe:**
```csharp
var metadata = new StockMetadata
{
    Authorization = new ChannelAuthorizationMetadata
    {
        IsEnabled = true,
        RequiredRoles = new[] { "Trader", "Admin" },
        RequiredClaims = new[] { "market-data-access" }
    }
};
```

### ChannelMessageEncryptionMetadata

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Metadata`

Message encryption configuration.

**Key Members:**
- `bool IsEnabled` - Encryption enabled
- `string Algorithm` - Encryption algorithm
- `string Key` - Encryption key

**Usage Recipe:**
```csharp
var metadata = new StockMetadata
{
    MessageEncryption = new ChannelMessageEncryptionMetadata
    {
        IsEnabled = true,
        Algorithm = "AES256",
        Key = "your-encryption-key"
    }
};
```

### ChannelSnapshotMetadata

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.Metadata`

Snapshot behavior configuration.

**Key Members:**
- `bool IsEnabled` - Snapshot enabled
- `bool IsTimeSeries` - Time-series mode (no updates, append-only)
- `TimeSpan? RetentionPeriod` - Snapshot retention period

**Usage Recipe:**
```csharp
var metadata = new StockMetadata
{
    Snapshot = new ChannelSnapshotMetadata
    {
        IsEnabled = true,
        IsTimeSeries = false, // Updates existing entries
        RetentionPeriod = TimeSpan.FromDays(30)
    }
};
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [IChannelMetadata.cs](../../src/ThunderPropagator.Application/Channels/Metadata/IChannelMetadata.cs) | 21 | Metadata interface |
| [AbstractChannelMetadata.cs](../../src/ThunderPropagator.Application/Channels/Metadata/AbstractChannelMetadata.cs) | 244 | Base metadata implementation |
| [ChannelAuthenticationMetadata.cs](../../src/ThunderPropagator.Application/Channels/Metadata/ChannelAuthenticationMetadata.cs) | 29 | Authentication configuration |
| [ChannelBasicUserConfiguration.cs](../../src/ThunderPropagator.Application/Channels/Metadata/ChannelBasicUserConfiguration.cs) | 19 | Basic auth credentials |
| [ChannelJwtConfiguration.cs](../../src/ThunderPropagator.Application/Channels/Metadata/ChannelJwtConfiguration.cs) | 10 | JWT configuration |
| [ChannelAuthorizationMetadata.cs](../../src/ThunderPropagator.Application/Channels/Metadata/ChannelAuthorizationMetadata.cs) | 20 | Authorization rules |
| [ChannelMessageEncryptionMetadata.cs](../../src/ThunderPropagator.Application/Channels/Metadata/ChannelMessageEncryptionMetadata.cs) | 20 | Encryption settings |
| [ChannelSnapshotMetadata.cs](../../src/ThunderPropagator.Application/Channels/Metadata/ChannelSnapshotMetadata.cs) | 38 | Snapshot configuration |

**Total Files:** 8  
**Total LOC:** 401

## Usage

### Complete Metadata Configuration

```csharp
public class TradingMetadata : AbstractChannelMetadata
{
    public override string ChannelName => "trading";
    public override string ChannelDisplayName => "Trading Channel";
    public override string ChannelDescription => "Real-time trading data";
    
    public TradingMetadata()
    {
        // Define fields
        ChannelProgramsDescriptors.Add(new SubscribingKeyChannelProgramsDescriptor
        {
            Name = "symbol",
            Index = 0,
            DisplayName = "Symbol"
        });
        
        ChannelProgramsDescriptors.Add(new DecimalChannelProgramsDescriptor
        {
            Name = "price",
            Index = 1,
            DisplayName = "Price",
            DecimalPlaces = 2
        });
        
        // Authentication
        Authentication = new ChannelAuthenticationMetadata
        {
            IsEnabled = true,
            JwtConfiguration = new ChannelJwtConfiguration
            {
                Issuer = "TradingPlatform",
                Audience = "TradingClients",
                SecretKey = "your-secret-key"
            }
        };
        
        // Authorization
        Authorization = new ChannelAuthorizationMetadata
        {
            IsEnabled = true,
            RequiredRoles = new[] { "Trader" }
        };
        
        // Encryption
        MessageEncryption = new ChannelMessageEncryptionMetadata
        {
            IsEnabled = true,
            Algorithm = "AES256",
            Key = "encryption-key"
        };
        
        // Snapshot
        Snapshot = new ChannelSnapshotMetadata
        {
            IsEnabled = true,
            IsTimeSeries = false,
            RetentionPeriod = TimeSpan.FromDays(7)
        };
    }
}
```

---

**Navigation:**  
[⬆️ Channels](../README.md) | [⬆️ Application Layer](../../README.md)

---

**Statistics:** 8 public types · 8 files · 401 LOC
