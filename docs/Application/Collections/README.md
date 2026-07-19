# Collections

> Specialized collection types for request/response handling

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Specialized dictionary-based collections for managing request query strings, form data, route parameters, and response content in a type-safe manner.

**Key Features:**
- 📋 Type-safe value retrieval
- 🔍 Query string parsing
- 📝 Form data handling
- 🛣️ Route table management
- 📤 Response content building

## Public Types

### IRequestContentFormCollection

**Kind:** Interface  
**Namespace:** `ThunderPropagator.Application.Collections`

Marker interface for request content form collections.

### RequestContentFormCollection

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Collections`

Dictionary-based collection for request form data with type-safe value retrieval.

**Key Members:**
- `T? GetValue<T>(string key)` - Get typed value
- `bool TryGetValue<T>(string key, out T? value)` - Try get typed value
- `void Add(string key, object? value)` - Add form value

**Usage Recipe:**
```csharp
var form = new RequestContentFormCollection();
form.Add("username", "john.doe");
form.Add("age", 30);

var username = form.GetValue<string>("username"); // "john.doe"
var age = form.GetValue<int>("age"); // 30
```

### ResponseContentFormCollection

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Collections`

Dictionary-based collection for building response content.

**Usage Recipe:**
```csharp
var response = new ResponseContentFormCollection();
response.Add("success", true);
response.Add("data", new { id = 123 });
```

### QueryStringsCollection

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Collections`

Parses and manages URL query string parameters.

**Key Members:**
- `T? GetValue<T>(string key)` - Get typed query parameter
- `bool ContainsKey(string key)` - Check parameter exists

**Usage Recipe:**
```csharp
var query = new QueryStringsCollection("?symbol=AAPL&limit=100");
var symbol = query.GetValue<string>("symbol"); // "AAPL"
var limit = query.GetValue<int>("limit"); // 100
```

### RouteTableCollection

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Collections`

Manages route parameters extracted from URL paths.

**Key Members:**
- `T? GetValue<T>(string key)` - Get typed route value
- `void Add(string key, string value)` - Add route parameter

**Usage Recipe:**
```csharp
var route = new RouteTableCollection();
route.Add("id", "123");
route.Add("action", "update");

var id = route.GetValue<int>("id"); // 123
```

### PushContentFormCollection

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Collections`

Collection for push message content.

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [IRequestContentFormCollection.cs](../../src/ThunderPropagator.Application/Collections/IRequestContentFormCollection.cs) | 4 | Request form interface |
| [RequestContentFormCollection.cs](../../src/ThunderPropagator.Application/Collections/RequestContentFormCollection.cs) | 89 | Request form data collection |
| [ResponseContentFormCollection.cs](../../src/ThunderPropagator.Application/Collections/ResponseContentFormCollection.cs) | 11 | Response content collection |
| [QueryStringsCollection.cs](../../src/ThunderPropagator.Application/Collections/QueryStringsCollection.cs) | 90 | Query string parser and collection |
| [RouteTableCollection.cs](../../src/ThunderPropagator.Application/Collections/RouteTableCollection.cs) | 137 | Route parameter collection |
| [PushContentFormCollection.cs](../../src/ThunderPropagator.Application/Collections/PushContentFormCollection.cs) | 18 | Push message content collection |

**Total Files:** 6  
**Total LOC:** 349

## Usage

### Working with Request Forms

```csharp
var request = new RequestContentFormCollection();
request.Add("username", "admin");
request.Add("password", "secret");
request.Add("remember_me", true);

if (request.TryGetValue<string>("username", out var username))
{
    Console.WriteLine($"Username: {username}");
}
```

### Parsing Query Strings

```csharp
var query = new QueryStringsCollection("?filter=active&page=2&size=50");

var filter = query.GetValue<string>("filter"); // "active"
var page = query.GetValue<int>("page"); // 2
var size = query.GetValue<int>("size"); // 50
```

### Building Responses

```csharp
var response = new ResponseContentFormCollection();
response.Add("status", "success");
response.Add("timestamp", DateTime.UtcNow);
response.Add("data", new[] { "item1", "item2" });
```

---

**Navigation:**  
[⬆️ Application Layer](../README.md)

---

**Statistics:** 6 public types · 6 files · 349 LOC
