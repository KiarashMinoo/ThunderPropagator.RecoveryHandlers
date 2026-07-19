using Dapper;
using ThunderPropagator.Application.Channels.Snapshots;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.RecoveryHandler.Postgresql
{
    partial class PostgresqlRecoveryHandler
    {
        protected override async Task InternalBackupAsync(CancellationToken cancellationToken = default)
        {
            var entries = await Channel
                .SearchSnapshotsAsync(snapshotEntry => snapshotEntry.State == SnapshotEntryState.Active, 0, 0, cancellationToken);

            if (entries.Length == 0)
                return;

            await using var connection = await OpenConnectionAsync(cancellationToken);
            await ExecuteInTransactionAsync(connection, async transaction =>
            {
                foreach (var batch in entries.Splice(100))
                {
                    await connection.ExecuteAsync(new CommandDefinition(
                        _snapshotEntryUpsertQuery,
                        batch.Select(e => new
                        {
                            e.HashKey,
                            Keys = e.Keys.ToNJson(),
                            CastType = (int)e.CastType,
                            State = (int)e.State,
                        }),
                        transaction: transaction,
                        cancellationToken: cancellationToken));

                    await connection.ExecuteAsync(new CommandDefinition(
                        _snapshotEntryItemsUpsertQuery,
                        batch.SelectMany(e => e.Snapshot.Select(item =>
                        {
                            var (type, value) = PostgresqlSnapshotTypeMapper.Serialize(item.Value);
                            return new { e.HashKey, item.Key, Type = type, Value = value };
                        })).ToList(),
                        transaction: transaction,
                        cancellationToken: cancellationToken));
                }
            }, Log.BackupFailed, cancellationToken);
        }
    }
}
