using Dapper;
using ThunderPropagator.Application.Channels.Snapshots;

namespace ThunderPropagator.RecoveryHandler.Postgresql
{
    partial class PostgresqlRecoveryHandler
    {
        protected override async Task InternalCleanupAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await ExecuteInTransactionAsync(connection, transaction =>
                connection.ExecuteAsync(new CommandDefinition(
                    $"""
                     TRUNCATE TABLE "{_schema}"."{_snapshotTableName}";
                     TRUNCATE TABLE "{_schema}"."{_tableName}";
                     """,
                    transaction: transaction,
                    cancellationToken: cancellationToken)),
                Log.CleanupFailed, cancellationToken);
        }

        protected override async Task InternalCleanupAsync(int hashKey, CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await ExecuteInTransactionAsync(connection, transaction =>
                connection.ExecuteAsync(new CommandDefinition(
                    $"""
                     DELETE FROM "{_schema}"."{_snapshotTableName}" WHERE "{nameof(SnapshotEntry.HashKey)}" = @{nameof(SnapshotEntry.HashKey)};
                     DELETE FROM "{_schema}"."{_tableName}" WHERE "{nameof(SnapshotEntry.HashKey)}" = @{nameof(SnapshotEntry.HashKey)};
                     """,
                    new { HashKey = hashKey },
                    transaction: transaction,
                    cancellationToken: cancellationToken)),
                Log.CleanupFailed, cancellationToken);
        }
    }
}
