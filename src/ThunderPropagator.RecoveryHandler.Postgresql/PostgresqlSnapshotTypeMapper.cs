using System.Globalization;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.RecoveryHandler.Postgresql
{
    internal static class PostgresqlSnapshotTypeMapper
    {
        internal static (string Type, string? Value) Serialize(object? value)
        {
            var type = value?.GetType();
            var typeCode = value switch
            {
                Enum => TypeCode.Object,
                _ => Type.GetTypeCode(type)
            };

            return (
                typeCode switch
                {
                    TypeCode.Object when type is not null => type.AssemblyQualifiedName!,
                    _ => typeCode.ToString()
                },
                value is not null
                    ? typeCode switch
                    {
                        TypeCode.Object => value.ToNJson(),
                        // Convert.ToString(object, IFormatProvider) routes through IConvertible.ToString(provider)
                        // for every primitive here, so numeric and date values always round-trip using
                        // invariant separators regardless of the server's OS/culture configuration.
                        // Without this, e.g. 1.5f serializes as "1,5" under a comma-decimal culture and
                        // fails to parse back on a machine configured with a dot-decimal culture.
                        _ => Convert.ToString(value, CultureInfo.InvariantCulture)
                    }
                    : string.Empty);
        }

        internal static object? Deserialize(string type, string value) => type switch
        {
            nameof(TypeCode.Boolean) => bool.Parse(value),
            nameof(TypeCode.Char) => value[0],
            nameof(TypeCode.Byte) => byte.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.SByte) => sbyte.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.Int16) => short.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.UInt16) => ushort.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.Int32) => int.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.UInt32) => uint.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.Int64) => long.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.UInt64) => ulong.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.Single) => float.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.Double) => double.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.Decimal) => decimal.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.DateTime) => DateTime.Parse(value, CultureInfo.InvariantCulture),
            nameof(TypeCode.String) => value,
            nameof(TypeCode.Empty) or nameof(TypeCode.DBNull) => null,
            _ => value.FromNJson(Type.GetType(type)
                ?? throw new InvalidOperationException(
                    $"Cannot resolve type '{type}' while deserializing a PostgreSQL snapshot entry. " +
                    "The type may have been renamed, moved to a different assembly, or the assembly may no longer be loaded."))
        };
    }
}
