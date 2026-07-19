using Dapper;
using Npgsql;
using ThunderPropagator.Application.Channels.Snapshots;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.RecoveryHandler.Postgresql
{
    partial class PostgresqlRecoveryHandler
    {
        private Task HibernateSnapshotEntryItemsAsync(
            int hashKey,
            IReadOnlyDictionary<string, object?> snapshot,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            CancellationToken cancellationToken)
            => connection.ExecuteAsync(new CommandDefinition(
                _snapshotEntryItemsUpsertQuery,
                snapshot.Select(item =>
                {
                    var (type, value) = PostgresqlSnapshotTypeMapper.Serialize(item.Value);
                    return new { HashKey = hashKey, item.Key, Type = type, Value = value };
                }).ToList(),
                transaction: transaction,
                cancellationToken: cancellationToken));

        protected override async Task InternalHibernateAsync(SnapshotEntry snapshotEntry, CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await ExecuteInTransactionAsync(connection, async transaction =>
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    _snapshotEntryUpsertQuery,
                    new
                    {
                        snapshotEntry.HashKey,
                        Keys = snapshotEntry.Keys.ToNJson(),
                        CastType = (int)snapshotEntry.CastType,
                        State = (int)snapshotEntry.State,
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

                await HibernateSnapshotEntryItemsAsync(snapshotEntry.HashKey, snapshotEntry.Snapshot, connection, transaction, cancellationToken);
            }, Log.HibernateFailed, cancellationToken);
        }

        protected override async Task InternalHibernateAsync(int hashKey, IReadOnlyDictionary<string, object?> snapshot, CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await ExecuteInTransactionAsync(connection, async transaction =>
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    $"""
                     UPDATE "{_schema}"."{_tableName}" SET
                         "{nameof(SnapshotEntry.State)}" = @{nameof(SnapshotEntry.State)}
                     WHERE
                         "{nameof(SnapshotEntry.HashKey)}" = @{nameof(SnapshotEntry.HashKey)}
                     """,
                    new { State = (int)SnapshotEntryState.Hibernated, HashKey = hashKey },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

                await HibernateSnapshotEntryItemsAsync(hashKey, snapshot, connection, transaction, cancellationToken);
            }, Log.HibernateFailed, cancellationToken);
        }
    }
}
