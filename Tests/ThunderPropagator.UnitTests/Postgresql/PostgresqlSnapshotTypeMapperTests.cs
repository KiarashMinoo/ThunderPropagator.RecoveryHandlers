using System.Globalization;
using FluentAssertions;
using ThunderPropagator.RecoveryHandler.Postgresql;

namespace ThunderPropagator.UnitTests.Postgresql;

public class PostgresqlSnapshotTypeMapperTests
{
    public static TheoryData<object?, string, string?> SerializationCases => new()
    {
        { null, nameof(TypeCode.Empty), string.Empty },
        { true, nameof(TypeCode.Boolean), bool.TrueString },
        { 42, nameof(TypeCode.Int32), "42" },
        { "thunder", nameof(TypeCode.String), "thunder" },
    };

    public static TheoryData<string, string, object?> DeserializationCases => new()
    {
        { nameof(TypeCode.Boolean), bool.TrueString, true },
        { nameof(TypeCode.Char), "T", 'T' },
        { nameof(TypeCode.Byte), "42", (byte)42 },
        { nameof(TypeCode.SByte), "-42", (sbyte)-42 },
        { nameof(TypeCode.Int16), "-42", (short)-42 },
        { nameof(TypeCode.UInt16), "42", (ushort)42 },
        { nameof(TypeCode.Int32), "-42", -42 },
        { nameof(TypeCode.UInt32), "42", 42U },
        { nameof(TypeCode.Int64), "-42", -42L },
        { nameof(TypeCode.UInt64), "42", 42UL },
        { nameof(TypeCode.Single), "42", 42F },
        { nameof(TypeCode.Double), "42", 42D },
        { nameof(TypeCode.Decimal), "42", 42M },
        { nameof(TypeCode.String), "thunder", "thunder" },
        { nameof(TypeCode.Empty), string.Empty, null },
        { nameof(TypeCode.DBNull), string.Empty, null },
    };

    [Theory]
    [MemberData(nameof(SerializationCases))]
    public void Serialize_PrimitiveOrNullValue_ReturnsTypeCodeAndText(
        object? value,
        string expectedType,
        string? expectedValue)
    {
        // Arrange

        // Act
        var result = PostgresqlSnapshotTypeMapper.Serialize(value);

        // Assert
        result.Type.Should().Be(expectedType);
        result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void Serialize_EnumValue_ReturnsAssemblyQualifiedTypeAndJson()
    {
        // Arrange
        const DayOfWeek value = DayOfWeek.Wednesday;

        // Act
        var result = PostgresqlSnapshotTypeMapper.Serialize(value);

        // Assert
        result.Type.Should().Be(typeof(DayOfWeek).AssemblyQualifiedName);
        PostgresqlSnapshotTypeMapper.Deserialize(result.Type, result.Value!)
            .Should().Be(value);
    }

    [Theory]
    [MemberData(nameof(DeserializationCases))]
    public void Deserialize_TypeCodeAndText_ReturnsTypedValue(
        string type,
        string value,
        object? expected)
    {
        // Arrange

        // Act
        var result = PostgresqlSnapshotTypeMapper.Deserialize(type, value);

        // Assert
        result.Should().Be(expected);
        result?.GetType().Should().Be(expected?.GetType());
    }

    [Fact]
    public void Deserialize_DateTimeText_ReturnsDateTimeValue()
    {
        // Arrange
        var expected = new DateTime(2026, 7, 19, 12, 30, 0, DateTimeKind.Unspecified);
        var value = expected.ToString("O");

        // Act
        var result = PostgresqlSnapshotTypeMapper.Deserialize(nameof(TypeCode.DateTime), value);

        // Assert
        result.Should().Be(expected);
    }

    public static TheoryData<object?> CultureSensitiveValues => new()
    {
        1.5F,
        1.5D,
        1.5M,
        new DateTime(2026, 7, 19, 12, 30, 0, DateTimeKind.Unspecified),
    };

    [Theory]
    [MemberData(nameof(CultureSensitiveValues))]
    public void SerializeThenDeserialize_FloatingPointAndDateValues_RoundTripUnderNonInvariantCulture(object value)
    {
        // Arrange — de-DE uses a comma as the decimal separator; if Serialize/Deserialize ever
        // used the current culture instead of CultureInfo.InvariantCulture, this value would
        // either serialize with a comma (corrupting anything reading it back under a
        // dot-decimal culture) or fail to parse back at all.
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

        try
        {
            // Act
            var (type, serialized) = PostgresqlSnapshotTypeMapper.Serialize(value);
            var roundTripped = PostgresqlSnapshotTypeMapper.Deserialize(type, serialized!);

            // Assert
            serialized.Should().NotContain(",",
                "the serialized form must use invariant (dot) separators regardless of CurrentCulture");
            roundTripped.Should().Be(value);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Deserialize_UnresolvableType_ThrowsInvalidOperationExceptionNamingTheType()
    {
        // Arrange
        const string unresolvableType = "ThunderPropagator.Tests.DoesNotExist, ThunderPropagator.Tests.MissingAssembly";

        // Act
        var act = () => PostgresqlSnapshotTypeMapper.Deserialize(unresolvableType, "{}");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{unresolvableType}*");
    }
}
