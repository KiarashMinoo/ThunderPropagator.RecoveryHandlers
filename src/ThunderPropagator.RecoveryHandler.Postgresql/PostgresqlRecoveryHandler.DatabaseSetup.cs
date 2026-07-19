using System.Data;
using System.Text.RegularExpressions;
using Npgsql;
using ThunderPropagator.Application.Channels.Snapshots;

namespace ThunderPropagator.RecoveryHandler.Postgresql
{
    partial class PostgresqlRecoveryHandler
    {
        private static async Task CreateDatabaseIfNotExistsAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            NpgsqlConnectionStringBuilder builder = new(connectionString);
            var dbName = builder.Database;

            if (string.IsNullOrEmpty(dbName))
                throw new ArgumentException("Database name cannot be null or empty.", nameof(connectionString));

            if (!Regex.IsMatch(dbName, @"^[\w\-]+$"))
                throw new ArgumentException(
                    $"Invalid database name '{dbName}': only alphanumeric characters, underscores, and hyphens are allowed.",
                    nameof(connectionString));

            builder.Database = "postgres";
            await using var connection = new NpgsqlConnection(builder.ConnectionString);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            await using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT 1 FROM PG_CATALOG.PG_DATABASE WHERE DATNAME = @dbName";
            checkCmd.Parameters.AddWithValue("dbName", dbName);

            var result = await checkCmd.ExecuteScalarAsync(cancellationToken);
            if (result is null)
            {
                await using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", connection);
                await createCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected override async Task InternalInitializeAsync(CancellationToken cancellationToken = default)
        {
            await CreateDatabaseIfNotExistsAsync(_connectionString, cancellationToken);

            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                                   BEGIN;

                                   CREATE SCHEMA IF NOT EXISTS "{_schema}";

                                   CREATE TABLE IF NOT EXISTS "{_schema}"."{_tableName}" (
                                       "{nameof(SnapshotEntry.HashKey)}" INT PRIMARY KEY,
                                       "{nameof(SnapshotEntry.Keys)}" TEXT NOT NULL,
                                       "{nameof(SnapshotEntry.CastType)}" INT NOT NULL,
                                       "{nameof(SnapshotEntry.State)}" INT NOT NULL
                                   );

                                   CREATE TABLE IF NOT EXISTS "{_schema}"."{_snapshotTableName}" (
                                       "{nameof(SnapshotEntry.HashKey)}" INT NOT NULL,
                                       "{nameof(KeyValuePair<string, object?>.Key)}" VARCHAR(256) NOT NULL,
                                       "{nameof(Type)}" TEXT NOT NULL,
                                       "{nameof(KeyValuePair<string, object?>.Value)}" TEXT NOT NULL,
                                       PRIMARY KEY
                                       (
                                           "{nameof(SnapshotEntry.HashKey)}",
                                           "{nameof(KeyValuePair<string, object?>.Key)}"
                                       ),
                                       FOREIGN KEY("{nameof(SnapshotEntry.HashKey)}")
                                           REFERENCES "{_schema}"."{_tableName}"("{nameof(SnapshotEntry.HashKey)}")
                                           ON DELETE CASCADE
                                   );

                                   COMMIT;
                                   """;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
