# LicenseManagers

> License validation for feature-gated components

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

License validation system for controlling access to premium features and components. Uses interop with native license validation library.

**Key Features:**
- 🔐 Feature-based licensing
- ✅ Type-based validation
- 🔌 IFeature marker interface integration
- 🛡️ Native interop for validation

## Public Types

### LicenseManagerInterop

**Kind:** Static Class  
**Namespace:** `ThunderPropagator.Application.LicenseManagers`

Provides license validation through native interop.

**Key Members:**
- `static bool IsAllowed(Type featureType)` - Check if type is licensed
- `static bool IsFeatureEnabled(string featureName)` - Check if feature is enabled by name

**Usage Recipe:**
```csharp
// Automatically checked in AbstractFeeder.StartingAsync
public class PremiumFeeder : AbstractFeeder<MyChannel, MyMessage, MyConfig>, IFeature
{
    // If !LicenseManagerInterop.IsAllowed(typeof(PremiumFeeder))
    // then StartingAsync returns early
}

// Manual check
if (LicenseManagerInterop.IsAllowed(typeof(MyPremiumFeature)))
{
    // Feature is licensed
}
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [LicenseManagerInterop.cs](../../src/ThunderPropagator.Application/LicenseManagers/LicenseManagerInterop.cs) | 198 | Native license validation interop |

**Total Files:** 1  
**Total LOC:** 198

## Usage

### Feature-Gated Component

```csharp
// Mark with IFeature for automatic validation
public class AdvancedAnalyticsFeeder : IterativeFeeder<MyChannel, MyMessage, MyConfig>, IFeature
{
    // Feeder will not start if license doesn't include this feature type
}
```

### Manual License Check

```csharp
public class MyService
{
    public void EnablePremiumFeatures()
    {
        if (!LicenseManagerInterop.IsFeatureEnabled("premium"))
        {
            throw new InvalidOperationException("Premium features not licensed");
        }
        
        // Enable features
    }
}
```

---

**Navigation:**  
[⬆️ Application Layer](../README.md)

---

**Statistics:** 1 public type · 1 file · 198 LOC
