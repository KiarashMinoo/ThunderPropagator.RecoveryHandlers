using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ThunderPropagator.Application.Channels;
using ThunderPropagator.Application.Channels.Snapshots;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.RecoveryHandler.Redis
{
    internal
#if !DEBUG
        sealed
#endif
        partial class RedisRecoveryHandler : AbstractRecoveryHandler
    {
        private readonly string _redisKey;
        private readonly string _connectionString;
        private readonly RedisConnectionMultiplexerCache _multiplexerCache;
        private IDatabase? _redisDatabase;

        private IDatabase RedisDatabase => _redisDatabase
            ?? throw new InvalidOperationException(
                $"{nameof(RedisRecoveryHandler)} has not been initialized — IRecoveryHandler.Initialize must complete before Backup/Restore/Cleanup/Hibernate can run.");

        public RedisRecoveryHandler(IServiceProvider serviceProvider, IChannel channel) : base(serviceProvider, channel)
        {
            _redisKey = channel.Metadata.ChannelName;
            _connectionString = Guard.Against.NullOrWhiteSpace(channel.Metadata.Snapshot.ConnectionString);
            _multiplexerCache = serviceProvider.GetRequiredService<RedisConnectionMultiplexerCache>();
        }

        protected override async Task InternalInitializeAsync(CancellationToken cancellationToken = default)
        {
            var multiplexer = await _multiplexerCache.GetOrCreateAsync(_connectionString, cancellationToken);
            _redisDatabase = multiplexer.GetDatabase();
        }

        protected override async Task InternalBackupAsync(CancellationToken cancellationToken = default)
        {
            var snapshotEntries = await Channel.SearchSnapshotsAsync(snapshotEntry => snapshotEntry.State == SnapshotEntryState.Active, 0, 0, cancellationToken);
            await RedisDatabase.HashSetAsync(_redisKey,
                snapshotEntries.Select(snapshotEntry => new HashEntry(snapshotEntry.HashKey, snapshotEntry.ToNJson())).ToArray());
        }

        protected override async Task InternalRestoreAsync(CancellationToken cancellationToken = default)
        {
            foreach (var hashEntry in await RedisDatabase.HashGetAllAsync(_redisKey))
            {
                var snapshotEntry = hashEntry.Value.ToString().FromNJson<SnapshotEntry>();
                if (snapshotEntry is not null && snapshotEntry.State == SnapshotEntryState.Active)
                    OverwriteSnapshot(snapshotEntry);
                else if (snapshotEntry is null)
                    Log.SnapshotDeserializationFailed(Logger, hashEntry.Name.ToString(), _redisKey);
            }
        }

        protected override async Task<SnapshotEntry?> InternalRestoreAsync(int hashKey, CancellationToken cancellationToken = default)
        {
            var hashEntry = await RedisDatabase.HashGetAsync(_redisKey, hashKey);
            if (!hashEntry.HasValue)
                return null;

            var snapshotEntry = hashEntry.ToString().FromNJson<SnapshotEntry>();
            if (snapshotEntry is null)
                Log.SnapshotDeserializationFailed(Logger, hashKey.ToString(), _redisKey);

            return snapshotEntry;
        }

        protected override Task InternalCleanupAsync(CancellationToken cancellationToken = default)
            => RedisDatabase.KeyDeleteAsync(_redisKey);

        protected override Task InternalCleanupAsync(int hashKey, CancellationToken cancellationToken = default)
            => RedisDatabase.HashDeleteAsync(_redisKey, hashKey);

        protected override Task InternalHibernateAsync(SnapshotEntry snapshotEntry, CancellationToken cancellationToken = default)
            => RedisDatabase.HashSetAsync(_redisKey, snapshotEntry.HashKey, snapshotEntry.ToNJson());

        protected override async Task InternalHibernateAsync(int hashKey, IReadOnlyDictionary<string, object?> snapshot, CancellationToken cancellationToken = default)
        {
            var hashEntry = await RedisDatabase.HashGetAsync(_redisKey, hashKey);
            if (hashEntry.HasValue)
            {
                var snapshotEntry = hashEntry.ToString().FromNJson<SnapshotEntry>();
                if (snapshotEntry is not null)
                {
                    SetSnapshot(snapshotEntry, snapshotEntry.CastType, snapshot);
                    await InternalHibernateAsync(snapshotEntry, cancellationToken);
                }
                else
                    Log.SnapshotDeserializationFailed(Logger, hashKey.ToString(), _redisKey);
            }
        }

        // Note: the underlying IConnectionMultiplexer is owned and disposed by
        // RedisConnectionMultiplexerCache (shared across handlers), not by this instance —
        // disposing it here would break every other channel sharing the same Redis server.

        private static partial class Log
        {
            [LoggerMessage(EventId = 12001, Level = LogLevel.Warning,
                Message = "Snapshot entry for hash field {HashKey} in Redis hash '{RedisKey}' could not be deserialized and was skipped.")]
            public static partial void SnapshotDeserializationFailed(ILogger logger, string hashKey, string redisKey);
        }
    }
}
