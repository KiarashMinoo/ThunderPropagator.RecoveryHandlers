# Helpers

> Utility helpers for scripting, message handling, and request processing

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Utility helper classes for common operations including C# script evaluation, feeder message manipulation, and request context helpers.

**Key Features:**
- 📜 C# script compilation and evaluation
- 📝 Message field manipulation
- 🔍 Request context extraction

## Public Types

### CSharpScriptHelper

**Kind:** Static Class  
**Namespace:** `ThunderPropagator.Application.Helpers`

Compiles and evaluates C# scripts at runtime using Roslyn.

**Key Members:**
- `static T Evaluate<T>(string script)` - Compile and evaluate script

**Usage Recipe:**
```csharp
var script = @"(x, y) => x + y";
var func = CSharpScriptHelper.Evaluate<Func<int, int, int>>(script);
var result = func(5, 3); // 8

// Channel event script
var messageScript = @"
    (channel, message) => {
        message[""timestamp""] = DateTime.UtcNow;
    }
";
var action = CSharpScriptHelper.Evaluate<Action<IChannel, IReadOnlyDictionary<string, object?>>>(messageScript);
```

### FeederMessageHelper

**Kind:** Static Class  
**Namespace:** `ThunderPropagator.Application.Helpers`

Helper methods for manipulating `FeederMessage` fields and formatting.

**Key Members:**
- `static string ReplaceSplitters(string value)` - Replace delimiter characters
- `static object? GetFieldValue(FeederMessage message, string fieldName)` - Get field value
- `static void SetFieldValue(FeederMessage message, string fieldName, object? value)` - Set field value

**Usage Recipe:**
```csharp
var message = new FeederMessage();

// Set field
FeederMessageHelper.SetFieldValue(message, "symbol", "AAPL");

// Get field
var symbol = FeederMessageHelper.GetFieldValue(message, "symbol"); // "AAPL"

// Replace delimiters in values
var safe = FeederMessageHelper.ReplaceSplitters("value,with,commas"); // Escapes commas
```

### RequestContextHelper

**Kind:** Static Class  
**Namespace:** `ThunderPropagator.Application.Helpers`

Helper methods for extracting information from request contexts.

**Key Members:**
- `static string GetConnectionId(RequestContext context)` - Extract connection ID
- `static ClaimsPrincipal GetUser(RequestContext context)` - Get authenticated user

**Usage Recipe:**
```csharp
public class MyPipeline : AbstractReceivePipeline<MyChannel>
{
    public async Task InvokeAsync(ReceiveContext context, ReceivePipelineDelegate next)
    {
        var connectionId = RequestContextHelper.GetConnectionId(context.Request);
        var user = RequestContextHelper.GetUser(context.Request);
        
        Logger.LogInformation(
            "Request from connection {ConnectionId}, user {User}",
            connectionId,
            user.Identity?.Name
        );
        
        await next(context);
    }
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [CSharpScriptHelper.cs](../../src/ThunderPropagator.Application/Helpers/CSharpScriptHelper.cs) | 32 | C# script compilation and evaluation |
| [FeederMessageHelper.cs](../../src/ThunderPropagator.Application/Helpers/FeederMessageHelper.cs) | 77 | Feeder message manipulation |
| [RequestContextHelper.cs](../../src/ThunderPropagator.Application/Helpers/RequestContextHelper.cs) | 15 | Request context extraction |

**Total Files:** 3  
**Total LOC:** 124

## Usage

### Evaluating Configuration Scripts

```csharp
public class MyChannelConfiguration : AbstractChannelConfiguration
{
    public MyChannelConfiguration()
    {
        Events.MessageEmitting = @"
            (channel, message) => {
                // Add server timestamp
                message[""server_time""] = DateTime.UtcNow;
                
                // Add channel name
                message[""channel""] = channel.Metadata.ChannelName;
            }
        ";
    }
}

// Script is evaluated in AbstractChannel constructor using CSharpScriptHelper
```

### Working with Feeder Messages

```csharp
var message = new FeederMessage
{
    ["symbol"] = "AAPL",
    ["price"] = 150.25m,
    ["volume"] = 1000000
};

// Replace delimiters in string values
foreach (var key in message.Keys.ToList())
{
    if (message[key] is string str)
    {
        message[key] = FeederMessageHelper.ReplaceSplitters(str);
    }
}
```

---

**Navigation:**  
[⬆️ Application Layer](../README.md)

---

**Statistics:** 3 public types · 3 files · 124 LOC
