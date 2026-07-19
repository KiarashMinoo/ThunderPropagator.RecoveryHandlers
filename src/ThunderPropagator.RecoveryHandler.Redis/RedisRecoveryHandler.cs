using Ardalis.GuardClauses;
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
        class RedisRecoveryHandler : AbstractRecoveryHandler
    {
        private readonly string _redisKey;
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _redisDatabase;

        public RedisRecoveryHandler(IServiceProvider serviceProvider, IChannel channel) : base(serviceProvider, channel)
        {
            _redisKey = channel.Metadata.ChannelName;
            _connectionMultiplexer = ConnectionMultiplexer.Connect(Guard.Against.NullOrWhiteSpace(channel.Metadata.Snapshot.ConnectionString));
            _redisDatabase = _connectionMultiplexer.GetDatabase();
        }

        protected override async Task InternalBackupAsync(CancellationToken cancellationToken = default)
        {
            var snapshotEntries = await Channel.SearchSnapshotsAsync(snapshotEntry => snapshotEntry.State == SnapshotEntryState.Active, 0, 0, cancellationToken);
            await _redisDatabase.HashSetAsync(_redisKey,
                snapshotEntries.Select(snapshotEntry => new HashEntry(snapshotEntry.HashKey, snapshotEntry.ToNJson())).ToArray());
        }

        protected override Task InternalRestoreAsync(CancellationToken cancellationToken = default)
        {
            foreach (var hashEntry in _redisDatabase.HashGetAll(_redisKey))
            {
                var snapshotEntry = hashEntry.Value.ToString().FromNJson<SnapshotEntry>();
                if (snapshotEntry is not null && snapshotEntry.State == SnapshotEntryState.Active)
                    OverwriteSnapshot(snapshotEntry);
            }

            return Task.CompletedTask;
        }

        protected override async Task<SnapshotEntry?> InternalRestoreAsync(int hashKey, CancellationToken cancellationToken = default)
        {
            var hashEntry = await _redisDatabase.HashGetAsync(_redisKey, hashKey);
            return hashEntry.HasValue ? hashEntry.ToString().FromNJson<SnapshotEntry>() : null;
        }

        protected override Task InternalCleanupAsync(CancellationToken cancellationToken = default)
            => _redisDatabase.KeyDeleteAsync(_redisKey);

        protected override Task InternalCleanupAsync(int hashKey, CancellationToken cancellationToken = default)
            => _redisDatabase.HashDeleteAsync(_redisKey, hashKey);

        protected override Task InternalHibernateAsync(SnapshotEntry snapshotEntry, CancellationToken cancellationToken = default)
            => _redisDatabase.HashSetAsync(_redisKey, snapshotEntry.HashKey, snapshotEntry.ToNJson());

        protected override async Task InternalHibernateAsync(int hashKey, IReadOnlyDictionary<string, object?> snapshot, CancellationToken cancellationToken = default)
        {
            var hashEntry = await _redisDatabase.HashGetAsync(_redisKey, hashKey);
            if (hashEntry.HasValue)
            {
                var snapshotEntry = hashEntry.ToString().FromNJson<SnapshotEntry>();
                if (snapshotEntry is not null)
                {
                    SetSnapshot(snapshotEntry, snapshotEntry.CastType, snapshot);
                    await InternalHibernateAsync(snapshotEntry, cancellationToken);
                }
            }
        }

        protected override async ValueTask DisposeManagedResourcesAsync()
        {
            await _connectionMultiplexer.DisposeAsync();
        }
    }
}
