using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pluralize.NET.Core;
using ThunderPropagator.Application.Channels;
using ThunderPropagator.Application.Channels.Snapshots;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;

namespace ThunderPropagator.RecoveryHandler.Postgresql
{
    internal
#if !DEBUG
        sealed
#endif
        partial class PostgresqlRecoveryHandler : AbstractRecoveryHandler
    {
        private readonly string _schema;
        private readonly string _tableName;
        private readonly string _snapshotTableName;
        private readonly string _connectionString;
        private readonly string _snapshotEntryUpsertQuery;
        private readonly string _snapshotEntryItemsUpsertQuery;

        public PostgresqlRecoveryHandler(IServiceProvider serviceProvider, IChannel channel)
            : base(serviceProvider, channel)
        {
            var channelName = channel.Metadata.ChannelName;

            if (string.IsNullOrWhiteSpace(channelName))
                throw new ArgumentNullException(nameof(channelName), "Channel name cannot be null.");

            if (!Regex.IsMatch(channelName, @"^[\w\-]+$"))
                throw new ArgumentException(
                    $"Invalid channel name '{channelName}': only alphanumeric characters, underscores, and hyphens are allowed for PostgreSQL table names.");

            _schema = nameof(Application.Channels.Snapshots);
            _tableName = channelName;
            _snapshotTableName = $"{channelName}_{new Pluralizer().Pluralize(nameof(SnapshotEntry))}";
            _connectionString = channel.Metadata.Snapshot.ConnectionString!;

            _snapshotEntryUpsertQuery = $"""
                                         INSERT INTO "{_schema}"."{_tableName}"
                                         (
                                             "{nameof(SnapshotEntry.HashKey)}",
                                             "{nameof(SnapshotEntry.Keys)}",
                                             "{nameof(SnapshotEntry.CastType)}",
                                             "{nameof(SnapshotEntry.State)}"
                                         )
                                         VALUES
                                         (
                                             @{nameof(SnapshotEntry.HashKey)},
                                             @{nameof(SnapshotEntry.Keys)},
                                             @{nameof(SnapshotEntry.CastType)},
                                             @{nameof(SnapshotEntry.State)}
                                         )
                                         ON CONFLICT("{nameof(SnapshotEntry.HashKey)}")
                                         DO UPDATE SET
                                             "{nameof(SnapshotEntry.Keys)}" = @{nameof(SnapshotEntry.Keys)},
                                             "{nameof(SnapshotEntry.CastType)}" = @{nameof(SnapshotEntry.CastType)},
                                             "{nameof(SnapshotEntry.State)}" = @{nameof(SnapshotEntry.State)};
                                         """;

            _snapshotEntryItemsUpsertQuery = $"""
                                              INSERT INTO "{_schema}"."{_snapshotTableName}"
                                              (
                                                 "{nameof(SnapshotEntry.HashKey)}",
                                                 "{nameof(KeyValuePair<string, object?>.Key)}",
                                                 "{nameof(Type)}",
                                                 "{nameof(KeyValuePair<string, object?>.Value)}"
                                              )
                                              VALUES
                                              (
                                                 @{nameof(SnapshotEntry.HashKey)},
                                                 @{nameof(KeyValuePair<string, object?>.Key)},
                                                 @{nameof(Type)},
                                                 @{nameof(KeyValuePair<string, object?>.Value)}
                                              )
                                              ON CONFLICT("{nameof(SnapshotEntry.HashKey)}", "{nameof(KeyValuePair<string, object?>.Key)}")
                                              DO UPDATE SET
                                                  "{nameof(Type)}" = @{nameof(Type)},
                                                  "{nameof(KeyValuePair<string, object?>.Value)}" = @{nameof(KeyValuePair<string, object?>.Value)};
                                              """;
        }

        private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        private async Task ExecuteInTransactionAsync(
            NpgsqlConnection connection,
            Func<NpgsqlTransaction, Task> work,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                await work(transaction);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                // Message template is a runtime variable — [LoggerMessage] requires a compile-time constant; intentionally kept as-is.
                Logger.LogError(exception, errorMessage);
            }
        }
    }
}
