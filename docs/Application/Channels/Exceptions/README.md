# Exceptions

> Channel-specific exception types

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Domain-specific exceptions for channel operations including subscription errors, validation failures, and channel state issues.

**Key Features:**
- 🚫 Channel state exceptions
- 🔑 Key validation exceptions
- 📋 Field validation exceptions
- 🔢 Request ID validation

## Public Types

### ChannelIsNotEnabledException

**Kind:** Exception  
**Namespace:** `ThunderPropagator.Application.Channels.Exceptions`  
**Inherits:** `Exception`

Thrown when attempting to use a disabled channel.

**Usage Recipe:**
```csharp
if (!channel.ChannelConfiguration.IsEnabled)
{
    throw new ChannelIsNotEnabledException(channel.Metadata.ChannelName);
}
```

### DuplicatedKeyException

**Kind:** Exception  
**Namespace:** `ThunderPropagator.Application.Channels.Exceptions`  
**Inherits:** `Exception`

Thrown when duplicate key values are provided in subscription request.

**Usage Recipe:**
```csharp
if (keys.GroupBy(k => k).Any(g => g.Count() > 1))
{
    throw new DuplicatedKeyException("Duplicate key values not allowed");
}
```

### DuplicatedRequestIdException

**Kind:** Exception  
**Namespace:** `ThunderPropagator.Application.Channels.Exceptions`  
**Inherits:** `Exception`

Thrown when duplicate request ID is used for subscription.

**Usage Recipe:**
```csharp
if (existingSubscriptions.Any(s => s.RequestId == requestId))
{
    throw new DuplicatedRequestIdException(requestId);
}
```

### InvalidSubscribedFieldException

**Kind:** Exception  
**Namespace:** `ThunderPropagator.Application.Channels.Exceptions`  
**Inherits:** `Exception`

Thrown when invalid or non-existent field is specified in subscription.

**Key Members:**
- `string FieldName` - Invalid field name
- `string ChannelName` - Channel name

**Usage Recipe:**
```csharp
var descriptor = channel.Metadata.ChannelProgramsDescriptors
    .FirstOrDefault(d => d.Name == fieldName);
    
if (descriptor == null)
{
    throw new InvalidSubscribedFieldException(fieldName, channel.Metadata.ChannelName);
}
```

### InvalidSubscribedKeyException

**Kind:** Exception  
**Namespace:** `ThunderPropagator.Application.Channels.Exceptions`  
**Inherits:** `Exception`

Thrown when invalid or non-existent key is specified in subscription.

**Key Members:**
- `string KeyName` - Invalid key name
- `string ChannelName` - Channel name

**Usage Recipe:**
```csharp
var descriptor = channel.Metadata.ChannelProgramsDescriptors
    .FirstOrDefault(d => d.Name == keyName && d.IsSubscribingKey);
    
if (descriptor == null)
{
    throw new InvalidSubscribedKeyException(keyName, channel.Metadata.ChannelName);
}
```

### SubscribingKeyRequiredException

**Kind:** Exception  
**Namespace:** `ThunderPropagator.Application.Channels.Exceptions`  
**Inherits:** `Exception`

Thrown when required subscribing keys are missing from subscription request.

**Key Members:**
- `string[] RequiredKeys` - Required key names
- `string ChannelName` - Channel name

**Usage Recipe:**
```csharp
var requiredKeys = channel.Metadata.ChannelProgramsDescriptors
    .Where(d => d.IsSubscribingKey && d.IsRequired)
    .Select(d => d.Name)
    .ToArray();
    
if (!requiredKeys.All(k => providedKeys.ContainsKey(k)))
{
    throw new SubscribingKeyRequiredException(requiredKeys, channel.Metadata.ChannelName);
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [ChannelIsNotEnabledException.cs](../../src/ThunderPropagator.Application/Channels/Exceptions/ChannelIsNotEnabledException.cs) | 12 | Channel disabled exception |
| [DuplicatedKeyException.cs](../../src/ThunderPropagator.Application/Channels/Exceptions/DuplicatedKeyException.cs) | 6 | Duplicate key exception |
| [DuplicatedRequestIdException.cs](../../src/ThunderPropagator.Application/Channels/Exceptions/DuplicatedRequestIdException.cs) | 10 | Duplicate request ID exception |
| [InvalidSubscribedFieldException.cs](../../src/ThunderPropagator.Application/Channels/Exceptions/InvalidSubscribedFieldException.cs) | 19 | Invalid field exception |
| [InvalidSubscribedKeyException.cs](../../src/ThunderPropagator.Application/Channels/Exceptions/InvalidSubscribedKeyException.cs) | 19 | Invalid key exception |
| [SubscribingKeyRequiredException.cs](../../src/ThunderPropagator.Application/Channels/Exceptions/SubscribingKeyRequiredException.cs) | 18 | Required keys missing exception |

**Total Files:** 6  
**Total LOC:** 84

## Usage

### Validating Subscription Request

```csharp
public IEnumerable<Subscription> Subscribe(
    IConnectionInfo connectionInfo,
    string requestId,
    ISubscribeRequest subscribeRequest)
{
    // Check channel enabled
    if (!ChannelConfiguration.IsEnabled)
    {
        throw new ChannelIsNotEnabledException(Metadata.ChannelName);
    }
    
    // Check duplicate request ID
    if (Subscriptions.Any(s => s.ConnectionInfo == connectionInfo && s.RequestId == requestId))
    {
        throw new DuplicatedRequestIdException(requestId);
    }
    
    // Validate keys
    foreach (var key in subscribeRequest.SubscribingKeys.Keys)
    {
        var descriptor = Metadata.ChannelProgramsDescriptors
            .FirstOrDefault(d => d.Name == key && d.IsSubscribingKey);
            
        if (descriptor == null)
        {
            throw new InvalidSubscribedKeyException(key, Metadata.ChannelName);
        }
    }
    
    // Validate fields
    foreach (var field in subscribeRequest.SubscribingFields)
    {
        var descriptor = Metadata.ChannelProgramsDescriptors
            .FirstOrDefault(d => d.Name == field);
            
        if (descriptor == null)
        {
            throw new InvalidSubscribedFieldException(field, Metadata.ChannelName);
        }
    }
    
    // Check required keys
    var requiredKeys = Metadata.ChannelProgramsDescriptors
        .Where(d => d.IsSubscribingKey && d.IsRequired)
        .Select(d => d.Name)
        .ToArray();
        
    if (!requiredKeys.All(k => subscribeRequest.SubscribingKeys.ContainsKey(k)))
    {
        throw new SubscribingKeyRequiredException(requiredKeys, Metadata.ChannelName);
    }
    
    // Create subscriptions...
}
```

---

**Navigation:**  
[⬆️ Channels](../README.md) | [⬆️ Application Layer](../../README.md)

---

**Statistics:** 6 public types · 6 files · 84 LOC
