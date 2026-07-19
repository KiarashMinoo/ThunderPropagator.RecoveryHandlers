using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ThunderPropagator.Application.Channels;
using ThunderPropagator.Application.Channels.Snapshots;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.RecoveryHandler.MongoDb
{
    internal
#if !DEBUG
        sealed
#endif
        partial class MongoDbRecoveryHandler : AbstractRecoveryHandler
    {
        private readonly MongoUrl _mongoUrl;
        private readonly MongoClient _mongoClient;
        private readonly IMongoCollection<SnapshotEntry> _collection;

        public MongoDbRecoveryHandler(IServiceProvider serviceProvider, IChannel channel) : base(serviceProvider, channel)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(SnapshotEntry)))
            {
                BsonClassMap.RegisterClassMap<SnapshotEntry>(options =>
                {
                    options.AutoMap();
                    options.SetIgnoreExtraElements(true);

                    options.MapCreator(snapshotEntry => SetSnapshotEntry(snapshotEntry.HashKey, snapshotEntry.Keys, snapshotEntry.CastType, snapshotEntry.Snapshot));
                    options.MapIdField(snapshotEntry => snapshotEntry.HashKey);
                    options.MapMember(snapshotEntry => snapshotEntry.Keys);
                    options.MapMember(snapshotEntry => snapshotEntry.CastType);
                    options.MapMember(snapshotEntry => snapshotEntry.State);
                    options.MapMember(snapshotEntry => snapshotEntry.LastFetchDateTime);
                    options.MapMember(snapshotEntry => snapshotEntry.Snapshot);
                });
            }

            _mongoUrl = MongoUrl.Create(Guard.Against.NullOrWhiteSpace(channel.Metadata.Snapshot.ConnectionString));
            _mongoClient = new MongoClient(_mongoUrl);
            _collection = _mongoClient.GetDatabase(_mongoUrl.DatabaseName).GetCollection<SnapshotEntry>(channel.Metadata.ChannelName);
        }

        protected override async Task InternalBackupAsync(CancellationToken cancellationToken = default)
        {
            var entries = await Channel
                .SearchSnapshotsAsync(snapshotEntry => snapshotEntry.State == SnapshotEntryState.Active, 0, 0, cancellationToken);

            if (entries.Length != 0)
            {
                using var session = await _mongoClient.StartSessionAsync(cancellationToken: cancellationToken);
                var database = session.Client.GetDatabase(_mongoUrl.DatabaseName);
                var collection = database.GetCollection<SnapshotEntry>(Channel.Metadata.ChannelName);

                session.StartTransaction();

                try
                {
                    foreach (var snapshotEntries in entries.Splice(100))
                    {
                        await collection.BulkWriteAsync(snapshotEntries
                                .Select(snapshotEntry => new ReplaceOneModel<SnapshotEntry>(
                                    Builders<SnapshotEntry>.Filter.Where(x => x.HashKey == snapshotEntry.HashKey),
                                    snapshotEntry) { IsUpsert = true }),
                            cancellationToken: cancellationToken);
                    }

                    await session.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception exception)
                {
                    await session.AbortTransactionAsync(cancellationToken);
                    Log.BackupFailed(Logger, exception);
                }
            }
        }

        protected override async Task InternalRestoreAsync(CancellationToken cancellationToken = default)
        {
            using var cursor = await _collection
                .FindAsync(x => x.State == SnapshotEntryState.Active, new FindOptions<SnapshotEntry> { BatchSize = 100 }, cancellationToken);

            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var snapshotEntry in cursor.Current)
                {
                    OverwriteSnapshot(snapshotEntry);
                }
            }
        }

        protected override async Task<SnapshotEntry?> InternalRestoreAsync(int hashKey, CancellationToken cancellationToken = default)
            => await _collection.Find(x => x.HashKey == hashKey).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        protected override Task InternalCleanupAsync(CancellationToken cancellationToken = default)
            => _collection.DeleteManyAsync(FilterDefinition<SnapshotEntry>.Empty, cancellationToken);

        protected override Task InternalCleanupAsync(int hashKey, CancellationToken cancellationToken = default)
            => _collection.DeleteOneAsync(x => x.HashKey == hashKey, cancellationToken: cancellationToken);

        protected override Task InternalHibernateAsync(SnapshotEntry snapshotEntry, CancellationToken cancellationToken = default)
            => _collection.ReplaceOneAsync(x => x.HashKey == snapshotEntry.HashKey, snapshotEntry, new ReplaceOptions { IsUpsert = true }, cancellationToken);

        protected override async Task InternalHibernateAsync(int hashKey, IReadOnlyDictionary<string, object?> snapshot, CancellationToken cancellationToken = default)
        {
            var snapshotEntry = await _collection.Find(x => x.HashKey == hashKey).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (snapshotEntry is not null)
            {
                SetSnapshot(snapshotEntry, snapshotEntry.CastType, snapshot);
                await InternalHibernateAsync(snapshotEntry, cancellationToken);
            }
        }

        private static partial class Log
        {
            [LoggerMessage(EventId = 10001, Level = LogLevel.Error, Message = "Backing up recovery table has failed.")]
            public static partial void BackupFailed(ILogger logger, Exception exception);
        }
    }
}
