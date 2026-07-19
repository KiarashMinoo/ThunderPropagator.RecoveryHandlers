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
                        _ => value.ToString()
                    }
                    : string.Empty);
        }

        internal static object? Deserialize(string type, string value) => type switch
        {
            nameof(TypeCode.Boolean) => bool.Parse(value),
            nameof(TypeCode.Char) => value[0],
            nameof(TypeCode.Byte) => byte.Parse(value),
            nameof(TypeCode.SByte) => sbyte.Parse(value),
            nameof(TypeCode.Int16) => short.Parse(value),
            nameof(TypeCode.UInt16) => ushort.Parse(value),
            nameof(TypeCode.Int32) => int.Parse(value),
            nameof(TypeCode.UInt32) => uint.Parse(value),
            nameof(TypeCode.Int64) => long.Parse(value),
            nameof(TypeCode.UInt64) => ulong.Parse(value),
            nameof(TypeCode.Single) => float.Parse(value),
            nameof(TypeCode.Double) => double.Parse(value),
            nameof(TypeCode.Decimal) => decimal.Parse(value),
            nameof(TypeCode.DateTime) => DateTime.Parse(value),
            nameof(TypeCode.String) => value,
            nameof(TypeCode.Empty) or nameof(TypeCode.DBNull) => null,
            _ => value.FromNJson(Type.GetType(type)!)
        };
    }
}
