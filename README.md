# ThunderPropagator

**ThunderPropagator** is a cutting-edge software solution designed to redefine real-time data streaming. Our mission is to provide **effortless, blazingly fast, and cloud-native streaming capabilities** for maximum impact. This repository contains the foundational libraries, **ThunderPropagator.Application** and **ThunderPropagator.Infrastructure**, which empower developers to build scalable, high-performance streaming applications with ease.

These libraries support **.NET 9** and **.NET 8**, and are configured to work across multiple platforms, including **ARM64**, **x64**, **x86**, and **AnyCPU**. They are available as **NuGet packages** and can be installed from GitHub Packages:
**`https://nuget.pkg.github.com/KiarashMinoo/index.json`**.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Supported Platforms](#supported-platforms)
- [Installation](#installation)
- [License Enforcement](#license-enforcement)
- [Documentation](#documentation)
- [License](#license)

---

## Overview

ThunderPropagator is designed to revolutionize real-time data streaming by providing:

- **Effortless Integration**: Simple and intuitive APIs for seamless integration into your applications.
- **Blazingly Fast Performance**: Optimized for low-latency, high-throughput streaming.
- **Cloud-Native Architecture**: Built for modern cloud environments, enabling scalability and resilience.
- **Cross-Platform Support**: Compatible with ARM64, x64, x86, and AnyCPU platforms.

Whether you're building real-time analytics, live event processing, or IoT data pipelines, ThunderPropagator empowers you to deliver maximum impact with minimal effort.

---

## Features

- **Multi-Protocol Support**: WebSockets, MQTT 5.0, QUIC, WebTransport, and custom streaming protocols
- **Cross-Platform Support**: Works seamlessly on ARM64, x64, x86, and AnyCPU platforms
- **.NET Compatibility**: Fully compatible with .NET 9 and .NET 8
- **High Performance**: Optimized for low-latency, high-throughput streaming with backpressure control
- **Cloud-Native Architecture**: Built for modern cloud environments with built-in scalability and resilience
- **Real-time Channels**: Advanced channel management with subscription handling and message distribution
- **Comprehensive Documentation**: 100% coverage with 60+ documented modules
- **GitHub Packages**: Easily installable via GitHub Packages repository

---

## Supported Platforms

The projects support the following platforms:

- **ARM64**
- **x64**
- **x86**
- **AnyCPU**

Both **Debug** and **Release** configurations are available for all platforms.

---

## Installation

### Step 1: Add GitHub Packages NuGet Repository
To install the libraries as NuGet packages, you need to add the GitHub Packages repository to your NuGet configuration.

#### Using Visual Studio:
1. Open Visual Studio
2. Go to **Tools** > **NuGet Package Manager** > **Package Manager Settings**
3. Under **Package Sources**, click the **+** button to add a new source
4. Enter the following details:
   - **Name**: `GitHub-KiarashMinoo`
   - **Source**: `https://nuget.pkg.github.com/KiarashMinoo/index.json`
5. Click **Update** and then **OK**

#### Using the Command Line:
Add the NuGet source using the following command:
```bash
dotnet nuget add source --name GitHub-KiarashMinoo --source https://nuget.pkg.github.com/KiarashMinoo/index.json
```

#### Create or Update `nuget.config`
The repository includes a `nuget.config` file configured for GitHub Packages. If you need to create your own, here's the configuration:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <!-- Public nuget.org first -->
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
        <!-- OWNER = org or user that OWNS the package feed -->
        <add key="github" value="https://nuget.pkg.github.com/KiarashMinoo/index.json" />
    </packageSources>

    <!-- Put credentials here or (better) in user-level NuGet.config -->
    <packageSourceCredentials>
        <github>
            <add key="Username" value="KiarashMinoo" />
            <add key="ClearTextPassword" value="your-github-token" />
        </github>
    </packageSourceCredentials>

    <!-- Optional but recommended: only send your private IDs to GitHub -->
    <packageSourceMapping>
        <packageSource key="github">
            <package pattern="ThunderPropagator.*" />
        </packageSource>
        <packageSource key="nuget.org">
            <package pattern="*" />
        </packageSource>
    </packageSourceMapping>
</configuration>
```

Place the `nuget.config` file in the root of your solution or project directory. This ensures that all projects in the solution can access the GitHub Packages repository.

### Step 2: Verify the Configuration

To verify that the GitHub Packages repository is correctly configured, you can use the following command in the terminal:
```bash
dotnet nuget list source
```
This will list all configured NuGet sources. You should see something like this in the output:
```text
Registered Sources:
  1.  nuget.org [Enabled]
      https://api.nuget.org/v3/index.json
  2.  github [Enabled]
      https://nuget.pkg.github.com/KiarashMinoo/index.json
```

### Step 3: Install the NuGet Packages
You can now install the packages using the following commands:

For `ThunderPropagator.Application`:
```bash
dotnet add package ThunderPropagator.Application
```

For `ThunderPropagator.Infrastructure`:
```bash
dotnet add package ThunderPropagator.Infrastructure
```

Alternatively, you can install the packages via the NuGet Package Manager in Visual Studio.

## License Enforcement

ThunderPropagator uses a native `licenseManager` library (`LicenseManager.dll` / `libLicenseManager.so` / `libLicenseManager.dylib`) to enforce feature licensing at runtime. Feature checks are cached in-process and survive hot-reload — no application restart is needed when a license changes.

---

### Provider Types

| Provider | How it works |
|----------|-------------|
| **Demo** | Available in DEBUG builds only. Activates automatically via `LM_InitializeDemo`; unlocks all features without a license file. Never available in Release builds. |
| **Cloud** | A cloud key tied to your hardware ID is issued by the vendor portal. Features are validated via the native library against the cloud service. |
| **ActivationFile** | A `.lic` file issued by the vendor portal. Suitable for air-gapped or offline deployments. Pass the path in configuration (see below). |

---

### Registration Flow

```
Your server                        Vendor portal
     │                                   │
     │  1. Get hardware ID               │
     │  IHardwareIdProvider              │
     │  .GetHardwareId()  ─────────────► │
     │                                   │  2. Issue cloud key
     │  ◄── cloud key or .lic file ──── │     or activation file
     │                                   │
     │  3. Place .lic in config path     │
     │     or supply cloud key           │
     │                                   │
     │  4. UseThunderPropagator()        │
     │     calls Initialize()            │
     │     → LoadLicense(path)           │
```

**Step 1 — Retrieve the hardware ID** at application startup:

```csharp
// In Program.cs / Startup.cs after building the host:
var hardwareId = app.Services
    .GetRequiredService<IHardwareIdProvider>()
    .GetHardwareId();

Console.WriteLine($"Register this hardware ID with the vendor portal: {hardwareId}");
```

Alternatively query the built-in endpoint (requires `UseThunderPropagator()` to be called):

```
GET /thunderpropagator/v{version}/license/features
```

**Step 2 — Submit the hardware ID** to the vendor portal to obtain a cloud key or a `.lic` activation file.

**Step 3 — Configure the license path** in `appsettings.json`:

```json
{
  "LicenseManager": {
    "LicensePath": "license.lic"
  }
}
```

Or via environment variable:

```
LicenseManager__LicensePath=/app/licenses/license.lic
```

**Step 4 — Start the application.** `UseThunderPropagator()` calls `LicenseManagerInterop.Initialize()` automatically, which loads the license and populates the in-process feature cache.

---

### Hot-Reload Without Restart

The background `LicenseRefreshService` re-queries the native library on a configurable interval (default: every 15 minutes). To trigger an immediate reload, inject `ILicenseRefreshService` and call `RefreshAsync()`:

```csharp
await app.Services
    .GetRequiredService<ILicenseRefreshService>()
    .RefreshAsync();
```

To change the refresh interval:

```csharp
services.Configure<LicenseRefreshOptions>(opts =>
    opts.RefreshInterval = TimeSpan.FromMinutes(5));
```

---

### Licensed Features

All 18 features are enforced at runtime via `LicenseManagerInterop.IsAllowed<T>()`. The feature key used in license files is the fully-qualified class name shown below.

#### Subscription Capacity

| Feature class | Description | Tier |
|---------------|-------------|------|
| `ThousandSubscriptionsFeature` | Up to 1,000 concurrent subscriptions | Standard |
| `TenThousandSubscriptionsFeature` | Up to 10,000 concurrent subscriptions | Standard |
| `FiftyThousandSubscriptionsFeature` | Up to 50,000 concurrent subscriptions | Professional |
| `HundredThousandSubscriptionsFeature` | Up to 100,000 concurrent subscriptions | Professional |
| `UnlimitedSubscriptionsFeature` | Unlimited concurrent subscriptions (up to 1 M on 64-bit) | Enterprise |

#### Message Throughput

| Feature class | Description | Tier |
|---------------|-------------|------|
| `TenMessagesPerSecondFeature` | 10 messages per second per channel | Standard |
| `FiftyMessagesPerSecondFeature` | 50 messages per second per channel | Professional |
| `HundredMessagesPerSecondFeature` | 100 messages per second per channel | Enterprise |

#### Recovery Storage

| Feature class | Description | Tier |
|---------------|-------------|------|
| `RedisRecoveryStorageFeature` | Redis-backed snapshot recovery | Professional |
| `MongoDbRecoveryStorageFeature` | MongoDB-backed snapshot recovery | Professional |
| `PostgresqlRecoveryStorageFeature` | PostgreSQL-backed snapshot recovery | Professional |

#### Protocol Extensions

| Feature class | Description | Tier |
|---------------|-------------|------|
| `QuicConnectionFeature` | QUIC (HTTP/3) connection listener | Enterprise |
| `MqttConnectionFeature` | MQTT 5.0 broker | Enterprise |
| `WebTransportFeature` | WebTransport over HTTP/3 | Enterprise |

#### Pipeline Infrastructure

| Feature class | Description | Tier |
|---------------|-------------|------|
| `AddPushersEventsFeature` | Event-driven outbound push handlers | Community |
| `AddPushersPipelinesFeature` | Outbound message delivery pipelines | Community |
| `AddReceiversEventsFeature` | Event-driven inbound receive handlers | Community |
| `AddReceiversPipelinesFeatures` | Inbound message processing pipelines | Community |

> **Note:** Community-tier pipeline features are required for basic operation and are expected to be licensed in all deployments.

---

### Graceful Degradation

| Scenario | Behaviour |
|----------|-----------|
| **Native library absent** (`DllNotFoundException`) | All `IsAllowed<T>()` calls return `false`; `GetHardwareId()` returns `string.Empty`. ThunderPropagator continues to run at minimum capacity — no exception is thrown. |
| **Feature not licensed** | The gated code path is bypassed silently. Subscription capacity falls to 500 concurrent subscriptions; protocol listeners are not started; feeders that implement `IFeature` skip `StartingAsync()` without transitioning state. |
| **License expired** | The native expiry callback fires `InvalidateCache()`; all `IsAllowed<T>()` calls return `false` until a valid license is reloaded. |
| **Transient error during feature check** | `IsAllowed<T>()` returns `false` and logs a warning. The result is **not** cached so the next call retries against the native library. |

---

## Documentation

Comprehensive documentation is available in the [`docs/`](./docs/) directory. The documentation is auto-generated from source code with complete API references, architecture diagrams, usage examples, and best practices.

**[📚 Documentation Portal](./docs/README.md)** — Main entry point with quick start guide and architecture overview

### Documentation Catalog

The following areas are fully documented with types, files, diagrams, and examples:

#### Application Layer `Types:80+` `Files:112` `Diagrams:✓`

**[Application](docs/Application/README.md)** — Core abstractions for channels, feeders, pipelines, and subscriptions

- **[Channels](docs/Application/Channels/README.md)** `Types:8` `Files:13` `Diagrams:✓` — Real-time channel management with subscriptions and snapshots
  - [ChannelProgramsDescriptors](docs/Application/Channels/ChannelProgramsDescriptors/README.md) `Types:11` `Files:13` `Diagrams:✓` — Field schema definitions and type descriptors
    - [DataTypes](docs/Application/Channels/ChannelProgramsDescriptors/DataTypes/README.md) `Types:13` `Files:13` `Diagrams:✗` — Type-specific descriptor implementations
  - [Contexts](docs/Application/Channels/Contexts/README.md) `Types:4` `Files:4` `Diagrams:✓` — Request/response/push context objects
  - [Exceptions](docs/Application/Channels/Exceptions/README.md) `Types:6` `Files:6` `Diagrams:✗` — Channel-specific exception types
  - [Metadata](docs/Application/Channels/Metadata/README.md) `Types:5` `Files:8` `Diagrams:✓` — Authentication, authorization, snapshot configuration
  - [Snapshots](docs/Application/Channels/Snapshots/README.md) `Types:3` `Files:6` `Diagrams:✓` — State persistence and recovery handlers
    - [Recovery](docs/Application/Channels/Snapshots/Recovery/README.md) `Types:2` `Files:2` `Diagrams:✗` — Recovery handler abstractions
  - [Subscribers](docs/Application/Channels/Subscribers/README.md) `Types:8` `Files:8` `Diagrams:✓` — Subscription filtering and management
- **[Collections](docs/Application/Collections/README.md)** `Types:6` `Files:6` `Diagrams:✗` — Specialized collections for routing and forms
- **[Events](docs/Application/Events/README.md)** `Types:1` `Files:1` `Diagrams:✓` — Event interfaces and implementations
  - [Pushers](docs/Application/Events/Pushers/README.md) `Types:2` `Files:2` `Diagrams:✗` — Push event abstractions
  - [Receivers](docs/Application/Events/Receivers/README.md) `Types:2` `Files:2` `Diagrams:✗` — Receive event abstractions
- **[Feeders](docs/Application/Feeders/README.md)** `Types:10` `Files:12` `Diagrams:✓` — Data source abstractions feeding channels
- **[HealthChecks](docs/Application/HealthChecks/README.md)** `Types:2` `Files:2` `Diagrams:✗` — Health monitoring interfaces
- **[Helpers](docs/Application/Helpers/README.md)** `Types:3` `Files:3` `Diagrams:✗` — Utility classes (C# scripting, request context)
- **[LicenseManagers](docs/Application/LicenseManagers/README.md)** `Types:1` `Files:1` `Diagrams:✗` — License validation interop
- **[Logging](docs/Application/Logging/README.md)** `Types:1` `Files:1` `Diagrams:✗` — Logging utilities
- **[Metrics](docs/Application/Metrics/README.md)** `Types:3` `Files:3` `Diagrams:✗` — Telemetry and observability
- **[Pipelines](docs/Application/Pipelines/README.md)** `Types:1` `Files:1` `Diagrams:✓` — Middleware for request/response processing
  - [Pushers](docs/Application/Pipelines/Pushers/README.md) `Types:2` `Files:3` `Diagrams:✗` — Push pipeline abstractions
  - [Receivers](docs/Application/Pipelines/Receivers/README.md) `Types:4` `Files:7` `Diagrams:✓` — Receive pipeline abstractions
    - [Attributes](docs/Application/Pipelines/Receivers/Attributes/README.md) `Types:3` `Files:3` `Diagrams:✗` — Pipeline attribute decorators

#### Infrastructure Layer `Types:45+` `Files:68` `Diagrams:✓`

**[Infrastructure](docs/Infrastructure/README.md)** — Protocol implementations and connection management

- **[Channels](docs/Infrastructure/Channels/README.md)** `Types:7` `Files:11` `Diagrams:✓` — Infrastructure channel coordination
  - [Snapshots/Recovery](docs/Infrastructure/Channels/Snapshots/Recovery/README.md) `Types:5` `Files:5` `Diagrams:✓` — Concrete recovery implementations (Redis, MongoDB, PostgreSQL)
- **[Contexts](docs/Infrastructure/Contexts/README.md)** `Types:4` `Files:4` `Diagrams:✓` — Client request/response contexts
- **[Events](docs/Infrastructure/Events/README.md)** `Types:2` `Files:2` `Diagrams:✓` — Infrastructure event handlers
- **[Exceptions](docs/Infrastructure/Exceptions/README.md)** `Types:1` `Files:1` `Diagrams:✗` — Infrastructure-specific exceptions
- **[Extensions](docs/Infrastructure/Extensions/README.md)** `Types:12` `Files:13` `Diagrams:✓` — DI and service registration
- **[Feeders](docs/Infrastructure/Feeders/README.md)** `Types:5` `Files:5` `Diagrams:✓` — Infrastructure-level feeders
- **[Pipelines](docs/Infrastructure/Pipelines/README.md)** `Types:2` `Files:2` `Diagrams:✓` — Pipeline hierarchy invokers
- **[Protocols](docs/Infrastructure/Protocols/README.md)** `Types:13` `Files:13` `Diagrams:✓` — Protocol handlers and containers
  - [InfiniteDataStream](docs/Infrastructure/Protocols/InfiniteDataStream/README.md) `Types:3` `Files:3` `Diagrams:✗` — Custom binary streaming
  - [Mqtt](docs/Infrastructure/Protocols/Mqtt/README.md) `Types:3` `Files:3` `Diagrams:✓` — MQTT 5.0 broker integration
  - [Quic](docs/Infrastructure/Protocols/Quic/README.md) `Types:3` `Files:3` `Diagrams:✓` — QUIC protocol support
  - [WebSockets](docs/Infrastructure/Protocols/WebSockets/README.md) `Types:3` `Files:3` `Diagrams:✓` — WebSocket connection handling
  - [WebTransport](docs/Infrastructure/Protocols/WebTransport/README.md) `Types:3` `Files:3` `Diagrams:✓` — WebTransport over HTTP/3
- **[Pushers](docs/Infrastructure/Pushers/README.md)** `Types:2` `Files:2` `Diagrams:✗` — Push pipeline implementations
- **[Receivers](docs/Infrastructure/Receivers/README.md)** — Receive pipeline implementations
  - [Pipelines](docs/Infrastructure/Receivers/Pipelines/README.md) — Pipeline catalog and routing
    - [Authentication](docs/Infrastructure/Receivers/Pipelines/Authentication/README.md) `Types:1` `Files:1` `Diagrams:✓` — Authentication handlers (Basic, OAuth2)
    - [Authorization](docs/Infrastructure/Receivers/Pipelines/Authorization/README.md) `Types:1` `Files:1` `Diagrams:✓` — Authorization handlers (Role, Policy)
    - [PingPong](docs/Infrastructure/Receivers/Pipelines/PingPong/README.md) `Types:1` `Files:1` `Diagrams:✗` — Health check ping/pong
    - [RequestMetadata](docs/Infrastructure/Receivers/Pipelines/RequestMetadata/README.md) `Types:1` `Files:1` `Diagrams:✓` — Channel metadata retrieval
    - [Subscribe](docs/Infrastructure/Receivers/Pipelines/Subscribe/README.md) `Types:1` `Files:1` `Diagrams:✓` — Subscription management
    - [Unsubscribe](docs/Infrastructure/Receivers/Pipelines/Unsubscribe/README.md) `Types:1` `Files:1` `Diagrams:✓` — Unsubscription handling

#### Automation & Tooling

**[scripts](docs/scripts/README.md)** `Scripts:8` `Diagrams:✓` — PowerShell automation

- **CI/CD Scripts** — Automated versioning, building, and publishing
  - Delete-GitHubPackages.ps1 — Package cleanup with wildcard filtering
  - Generate-Changelog.ps1 — Keep-a-Changelog format generator
  - Generate-ReleaseNotes.ps1 — Advanced release notes with diff analysis
  - pack-all-platforms.ps1 — Parallel multi-platform builds
  - pack-solution.ps1 — Core .NET pack worker
  - publish-packages.ps1 — NuGet publishing with visibility control
  - update-version.ps1 — Beta/release version management

### Package Documentation

| Package | Version | Description | Documentation |
|---------|---------|-------------|---------------|
| ThunderPropagator.Application | 1.0.1-beta.12 | Core application layer with channels, feeders, pipelines | [Application Layer](docs/Application/README.md) |
| ThunderPropagator.Infrastructure | 1.0.1-beta.12 | Infrastructure layer with protocol implementations | [Infrastructure Layer](docs/Infrastructure/README.md) |
| ThunderPropagator.BuildingBlocks | 1.0.1-beta.12 | Core building blocks and utilities | Dependency |
| ThunderPropagator.BuildingBlocks.Modules | 1.0.1-beta.12 | Module system and DI extensions | Dependency |

### Documentation Features

- ✅ **Complete API Coverage** — All public types documented with signatures, descriptions, and usage
- ✅ **Architecture Diagrams** — Mermaid diagrams for flows, sequences, and class structures
- ✅ **Realistic Examples** — Production-ready code samples (no test code)
- ✅ **Cross-References** — Extensive linking between related documentation
- ✅ **Performance Notes** — Optimization tips and scalability guidance
- ✅ **Protocol Guides** — WebSocket, MQTT 5.0, QUIC, WebTransport details
- ✅ **Dependency Tracking** — ThunderPropagator package usage documented per area

**Last generated:** December 28, 2025

## License
This project is licensed under the **MIT License**.

© 2024-2025 ThunderPropagator. All rights reserved.
