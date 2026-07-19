using Dapper;
using ThunderPropagator.Application.Channels.Snapshots;
using ThunderPropagator.BuildingBlocks.Application.Enums;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.RecoveryHandler.Postgresql
{
    partial class PostgresqlRecoveryHandler
    {
        private async Task<ICollection<SnapshotEntry>> InternalRestoreAsync(
            bool onlyActives = true,
            int? hashKey = null,
            CancellationToken cancellationToken = default)
        {
            var selectQuery = $"""
                               SELECT t."{nameof(SnapshotEntry.HashKey)}",
                                      t."{nameof(SnapshotEntry.Keys)}",
                                      t."{nameof(SnapshotEntry.CastType)}",
                                      t."{nameof(SnapshotEntry.State)}",
                                      ts."{nameof(SnapshotEntry.HashKey)}" AS hk,
                                      ts."{nameof(KeyValuePair<string, object?>.Key)}",
                                      ts."{nameof(Type)}",
                                      ts."{nameof(KeyValuePair<string, object?>.Value)}"
                               FROM "{_schema}"."{_tableName}" t
                               INNER JOIN "{_schema}"."{_snapshotTableName}" ts ON t."{nameof(SnapshotEntry.HashKey)}" = ts."{nameof(SnapshotEntry.HashKey)}"
                               """;

            var whereClause = "WHERE ";
            if (onlyActives)
                whereClause += $"t.\"{nameof(SnapshotEntry.State)}\" = {(int)SnapshotEntryState.Active} AND ";

            if (hashKey is not null)
                whereClause += $"t.\"{nameof(SnapshotEntry.HashKey)}\" = @{nameof(SnapshotEntry.HashKey)} AND ";

            whereClause += "1 = 1";

            await using var connection = await OpenConnectionAsync(cancellationToken);

            Dictionary<int, SnapshotEntry> snapshotEntries = [];
            await connection.QueryAsync<
                (int HashKey, string Keys, int CastType, int State),
                (int HashKey, string Key, string Type, string Value),
                SnapshotEntry>(
                new CommandDefinition(
                    $"{selectQuery} {whereClause}",
                    new { HashKey = hashKey },
                    cancellationToken: cancellationToken),
                (entry, entryItem) =>
                {
                    var castType = (CastType)entry.CastType;
                    var feederMessage = new Dictionary<string, object?>
                    {
                        { entryItem.Key, PostgresqlSnapshotTypeMapper.Deserialize(entryItem.Type, entryItem.Value) }
                    };

                    if (!snapshotEntries.TryGetValue(entry.HashKey, out var snapshotEntry))
                    {
                        snapshotEntry = SetSnapshotEntry(
                            entry.HashKey,
                            entry.Keys.FromNJson<Dictionary<string, object?>>()!,
                            castType,
                            feederMessage);
                        snapshotEntries.Add(snapshotEntry.HashKey, snapshotEntry);
                    }
                    else
                        SetSnapshot(snapshotEntry, castType, feederMessage);

                    return snapshotEntry;
                }, splitOn: "hk");

            return snapshotEntries.Values;
        }

        protected override async Task InternalRestoreAsync(CancellationToken cancellationToken = default)
        {
            var snapshotEntries = await InternalRestoreAsync(cancellationToken: cancellationToken);
            foreach (var snapshotEntry in snapshotEntries)
                OverwriteSnapshot(snapshotEntry);
        }

        protected override async Task<SnapshotEntry?> InternalRestoreAsync(int hashKey, CancellationToken cancellationToken = default)
            => (await InternalRestoreAsync(false, hashKey, cancellationToken)).FirstOrDefault();
    }
}
