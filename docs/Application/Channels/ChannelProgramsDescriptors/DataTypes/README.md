# DataTypes

> Specialized field type descriptors

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Specialized descriptor types for specific data types with formatting, validation, and type-specific properties.

**Key Features:**
- 🔢 Numeric types (Number, Decimal, Currency, Percent)
- 📅 Temporal types (Date, DateTime, Time)
- ✅ Boolean type
- 🔑 Subscribing key type
- 📝 Enum type
- 📦 JSON type

## Public Types

### SubscribingKeyChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `ChannelProgramsDescriptor`

Marks a field as a subscribing key for subscription matching.

**Usage Recipe:**
```csharp
new SubscribingKeyChannelProgramsDescriptor
{
    Name = "symbol",
    Index = 0,
    DisplayName = "Symbol",
    IsRequired = true
}
```

### NumberChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `ChannelProgramsDescriptor`

Integer number field with min/max validation.

**Key Members:**
- `long? MinValue` - Minimum value
- `long? MaxValue` - Maximum value
- `string Format(object? value)` - Format as number

**Usage Recipe:**
```csharp
new NumberChannelProgramsDescriptor
{
    Name = "volume",
    Index = 1,
    DisplayName = "Volume",
    MinValue = 0,
    MaxValue = long.MaxValue
}
```

### DecimalChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `ChannelProgramsDescriptor`

Decimal number field with precision control.

**Key Members:**
- `int DecimalPlaces` - Number of decimal places
- `decimal? MinValue` - Minimum value
- `decimal? MaxValue` - Maximum value
- `string Format(object? value)` - Format with decimal places

**Usage Recipe:**
```csharp
new DecimalChannelProgramsDescriptor
{
    Name = "price",
    Index = 2,
    DisplayName = "Price",
    DecimalPlaces = 2,
    MinValue = 0
}
```

### CurrencyChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `DecimalChannelProgramsDescriptor`

Currency value with symbol and formatting.

**Key Members:**
- `string CurrencySymbol` - Currency symbol (e.g., "$", "€")
- `string CurrencyCode` - ISO currency code (e.g., "USD", "EUR")
- `string Format(object? value)` - Format as currency

**Usage Recipe:**
```csharp
new CurrencyChannelProgramsDescriptor
{
    Name = "amount",
    Index = 3,
    DisplayName = "Amount",
    DecimalPlaces = 2,
    CurrencySymbol = "$",
    CurrencyCode = "USD"
}
```

### PercentChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `DecimalChannelProgramsDescriptor`

Percentage value with formatting.

**Key Members:**
- `string Format(object? value)` - Format as percentage

**Usage Recipe:**
```csharp
new PercentChannelProgramsDescriptor
{
    Name = "change_percent",
    Index = 4,
    DisplayName = "Change %",
    DecimalPlaces = 2,
    MinValue = -100,
    MaxValue = 100
}
```

### DateChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `ChannelProgramsDescriptor`

Date-only field with format pattern.

**Key Members:**
- `string DateFormat` - Date format pattern (default: "yyyy-MM-dd")
- `string Format(object? value)` - Format as date string

**Usage Recipe:**
```csharp
new DateChannelProgramsDescriptor
{
    Name = "trade_date",
    Index = 5,
    DisplayName = "Trade Date",
    DateFormat = "yyyy-MM-dd"
}
```

### DateTimeChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `ChannelProgramsDescriptor`

Date and time field with format pattern.

**Key Members:**
- `string DateTimeFormat` - DateTime format pattern (default: "yyyy-MM-dd HH:mm:ss")
- `string Format(object? value)` - Format as datetime string

**Usage Recipe:**
```csharp
new DateTimeChannelProgramsDescriptor
{
    Name = "timestamp",
    Index = 6,
    DisplayName = "Timestamp",
    DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff"
}
```

### TimeChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `ChannelProgramsDescriptor`

Time-only field with format pattern.

**Key Members:**
- `string TimeFormat` - Time format pattern (default: "HH:mm:ss")
- `string Format(object? value)` - Format as time string

**Usage Recipe:**
```csharp
new TimeChannelProgramsDescriptor
{
    Name = "trade_time",
    Index = 7,
    DisplayName = "Trade Time",
    TimeFormat = "HH:mm:ss"
}
```

### BooleanChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `ChannelProgramsDescriptor`

Boolean field.

**Key Members:**
- `string Format(object? value)` - Format as "true"/"false"

**Usage Recipe:**
```csharp
new BooleanChannelProgramsDescriptor
{
    Name = "is_active",
    Index = 8,
    DisplayName = "Active"
}
```

### EnumChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `ChannelProgramsDescriptor`

Enumeration field with allowed values.

**Key Members:**
- `Type EnumType` - Enum type
- `Dictionary<int, string> Values` - Enum values
- `string Format(object? value)` - Format as enum name

**Usage Recipe:**
```csharp
public enum OrderStatus { Pending, Filled, Cancelled }

new EnumChannelProgramsDescriptor
{
    Name = "status",
    Index = 9,
    DisplayName = "Status",
    EnumType = typeof(OrderStatus),
    Values = new Dictionary<int, string>
    {
        [0] = "Pending",
        [1] = "Filled",
        [2] = "Cancelled"
    }
}
```

### JsonChannelProgramsDescriptor

**Kind:** Class  
**Namespace:** `ThunderPropagator.Application.Channels.ChannelProgramsDescriptors.DataTypes`  
**Inherits:** `ChannelProgramsDescriptor`

JSON object/array field.

**Key Members:**
- `string Format(object? value)` - Format as JSON string

**Usage Recipe:**
```csharp
new JsonChannelProgramsDescriptor
{
    Name = "metadata",
    Index = 10,
    DisplayName = "Metadata"
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [SubscribingKeyChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/SubscribingKeyChannelProgramsDescriptor.cs) | 16 | Subscribing key marker |
| [BooleanChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/BooleanChannelProgramsDescriptor.cs) | 21 | Boolean field |
| [NumberChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/NumberChannelProgramsDescriptor.cs) | 31 | Integer number field |
| [DecimalChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/DecimalChannelProgramsDescriptor.cs) | 28 | Decimal number field |
| [CurrencyChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/CurrencyChannelProgramsDescriptor.cs) | 26 | Currency field |
| [PercentChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/PercentChannelProgramsDescriptor.cs) | 26 | Percentage field |
| [DateChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/DateChannelProgramsDescriptor.cs) | 28 | Date-only field |
| [DateTimeChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/DateTimeChannelProgramsDescriptor.cs) | 25 | DateTime field |
| [TimeChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/TimeChannelProgramsDescriptor.cs) | 28 | Time-only field |
| [EnumChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/EnumChannelProgramsDescriptor.cs) | 27 | Enumeration field |
| [JsonChannelProgramsDescriptor.cs](../../src/ThunderPropagator.Application/Channels/ChannelProgramsDescriptors/DataTypes/JsonChannelProgramsDescriptor.cs) | 19 | JSON field |

**Total Files:** 11  
**Total LOC:** 313

## Usage

### Comprehensive Channel Schema

```csharp
public class TradingMetadata : AbstractChannelMetadata
{
    public override string ChannelName => "trading";
    
    public TradingMetadata()
    {
        ChannelProgramsDescriptors.Add(new SubscribingKeyChannelProgramsDescriptor
        {
            Name = "order_id",
            Index = 0,
            DisplayName = "Order ID",
            IsRequired = true
        });
        
        ChannelProgramsDescriptors.Add(new DecimalChannelProgramsDescriptor
        {
            Name = "price",
            Index = 1,
            DisplayName = "Price",
            DecimalPlaces = 2
        });
        
        ChannelProgramsDescriptors.Add(new NumberChannelProgramsDescriptor
        {
            Name = "quantity",
            Index = 2,
            DisplayName = "Quantity",
            MinValue = 1
        });
        
        ChannelProgramsDescriptors.Add(new CurrencyChannelProgramsDescriptor
        {
            Name = "total",
            Index = 3,
            DisplayName = "Total",
            DecimalPlaces = 2,
            CurrencySymbol = "$",
            CurrencyCode = "USD"
        });
        
        ChannelProgramsDescriptors.Add(new EnumChannelProgramsDescriptor
        {
            Name = "status",
            Index = 4,
            DisplayName = "Status",
            EnumType = typeof(OrderStatus),
            Values = new Dictionary<int, string>
            {
                [0] = "Pending",
                [1] = "Filled"
            }
        });
        
        ChannelProgramsDescriptors.Add(new DateTimeChannelProgramsDescriptor
        {
            Name = "timestamp",
            Index = 5,
            DisplayName = "Timestamp",
            DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff"
        });
    }
}
```

---

**Navigation:**  
[⬆️ ChannelProgramsDescriptors](../README.md) | [⬆️ Channels](../../README.md) | [⬆️ Application Layer](../../../README.md)

---

**Statistics:** 11 public types · 11 files · 313 LOC
