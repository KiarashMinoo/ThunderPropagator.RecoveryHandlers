# Recovery

> Snapshot backup and restore handlers

## Contents

- [Overview](#overview)
- [Public Types](#public-types)
- [Files](#files)
- [Usage](#usage)

## Overview

Recovery handlers provide backup and restore functionality for channel snapshots, enabling state persistence across application restarts.

**Key Features:**
- 💾 Snapshot backup to storage
- 🔄 Restore from storage
- 🧹 Cleanup old snapshots
- 🎯 Hangfire job integration

## Public Types

### IRecoveryHandler

**Kind:** Interface  
**Namespace:** `ThunderPropagator.Application.Channels.Snapshots.Recovery`

Interface for snapshot backup/restore operations.

**Key Members:**
- `Task BackupAsync(CancellationToken)` - Backup snapshots
- `Task<int> RestoreAsync(int? snapshotVersion, CancellationToken)` - Restore from backup
- `Task CleanupAsync(int? olderThanVersion, CancellationToken)` - Cleanup old snapshots

**Usage Recipe:**
```csharp
public class MyRecoveryHandler : IRecoveryHandler
{
    public async Task BackupAsync(CancellationToken cancellationToken)
    {
        // Save snapshots to database/file system
    }
    
    public async Task<int> RestoreAsync(int? snapshotVersion, CancellationToken cancellationToken)
    {
        // Load snapshots from storage
        return loadedVersion;
    }
    
    public async Task CleanupAsync(int? olderThanVersion, CancellationToken cancellationToken)
    {
        // Delete old backup files
    }
}
```

### AbstractRecoveryHandler

**Kind:** Abstract Class  
**Namespace:** `ThunderPropagator.Application.Channels.Snapshots.Recovery`  
**Implements:** `IRecoveryHandler`

Base implementation with Hangfire job scheduling for automatic backups.

**Key Members:**
- `IChannel Channel` - Associated channel
- `string RecurringJobId` - Hangfire job ID
- `abstract Task BackupAsync(CancellationToken)` - Override for backup logic
- `abstract Task<int> RestoreAsync(int?, CancellationToken)` - Override for restore logic
- `abstract Task CleanupAsync(int?, CancellationToken)` - Override for cleanup logic

**Usage Recipe:**
```csharp
public class FileSystemRecoveryHandler : AbstractRecoveryHandler
{
    private readonly string _backupPath;
    
    public FileSystemRecoveryHandler(IChannel channel, string backupPath)
        : base(channel)
    {
        _backupPath = backupPath;
    }
    
    protected override async Task BackupAsync(CancellationToken cancellationToken)
    {
        var snapshots = await Channel.SearchSnapshotsAsync(
            entries => entries,
            page: 0,
            pageSize: int.MaxValue,
            cancellationToken
        );
        
        var json = JsonSerializer.Serialize(snapshots);
        await File.WriteAllTextAsync(
            Path.Combine(_backupPath, $"snapshot_{DateTime.UtcNow:yyyyMMddHHmmss}.json"),
            json,
            cancellationToken
        );
    }
    
    protected override async Task<int> RestoreAsync(int? snapshotVersion, CancellationToken cancellationToken)
    {
        var files = Directory.GetFiles(_backupPath, "snapshot_*.json")
            .OrderByDescending(f => f)
            .ToArray();
            
        if (files.Length == 0) return 0;
        
        var json = await File.ReadAllTextAsync(files[0], cancellationToken);
        var snapshots = JsonSerializer.Deserialize<SnapshotEntry[]>(json);
        
        foreach (var snapshot in snapshots)
        {
            Channel.OverwriteSnapshot(snapshot);
        }
        
        return 1;
    }
    
    protected override Task CleanupAsync(int? olderThanVersion, CancellationToken cancellationToken)
    {
        var files = Directory.GetFiles(_backupPath, "snapshot_*.json");
        var cutoff = DateTime.UtcNow.AddDays(-7);
        
        foreach (var file in files)
        {
            if (File.GetCreationTimeUtc(file) < cutoff)
            {
                File.Delete(file);
            }
        }
        
        return Task.CompletedTask;
    }
}
```

### RecoveryHandlerResolver

**Kind:** Delegate  
**Namespace:** `ThunderPropagator.Application.Channels.Snapshots.Recovery`

Resolver for creating recovery handler instances.

**Signature:**
```csharp
public delegate IRecoveryHandler RecoveryHandlerResolver(IChannel channel);
```

## Files

| File | LOC | Responsibility |
|------|-----|---------------|
| [IRecoveryHandler.cs](../../src/ThunderPropagator.Application/Channels/Snapshots/Recovery/IRecoveryHandler.cs) | 24 | Recovery handler interface |
| [AbstractRecoveryHandler.cs](../../src/ThunderPropagator.Application/Channels/Snapshots/Recovery/AbstractRecoveryHandler.cs) | 282 | Base handler with Hangfire integration |
| [RecoveryHandlerResolver.cs](../../src/ThunderPropagator.Application/Channels/Snapshots/Recovery/RecoveryHandlerResolver.cs) | 3 | Resolver delegate |

**Total Files:** 3  
**Total LOC:** 309

## Usage

### Registering Recovery Handler

```csharp
// In channel initialization
services.AddSingleton<RecoveryHandlerResolver>(channel =>
{
    if (channel is StockChannel)
    {
        return new FileSystemRecoveryHandler(channel, @"C:\snapshots\stocks");
    }
    return null;
});
```

### Manual Backup/Restore

```csharp
var channel = serviceProvider.GetRequiredService<StockChannel>();
var recoveryHandler = new FileSystemRecoveryHandler(channel, @"C:\snapshots");

// Backup
await recoveryHandler.BackupAsync(CancellationToken.None);

// Restore latest
var version = await recoveryHandler.RestoreAsync(null, CancellationToken.None);
Console.WriteLine($"Restored version {version}");

// Cleanup old backups
await recoveryHandler.CleanupAsync(olderThanVersion: version - 5, CancellationToken.None);
```

### Automatic Scheduled Backups

```csharp
// AbstractRecoveryHandler automatically schedules backups via Hangfire
// Default schedule: Daily at midnight
// Override BackupCronExpression property to customize

public class MyRecoveryHandler : AbstractRecoveryHandler
{
    protected override string BackupCronExpression => Cron.Hourly(); // Backup every hour
    
    // ... implementation
}
```

---

**Navigation:**  
[⬆️ Snapshots](../README.md) | [⬆️ Channels](../../README.md) | [⬆️ Application Layer](../../../README.md)

---

**Statistics:** 3 public types · 3 files · 309 LOC
