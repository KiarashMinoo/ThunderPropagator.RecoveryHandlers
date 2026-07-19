using FluentAssertions;
using MongoDB.Driver;
using ThunderPropagator.RecoveryHandler.MongoDb;

namespace ThunderPropagator.UnitTests.MongoDb;

public class MongoClientCacheTests
{
    private const string ConnectionStringA = "mongodb://localhost:27017/db-a";
    private const string ConnectionStringB = "mongodb://localhost:27017/db-b";

    [Fact]
    public void GetOrCreate_SameConnectionStringCalledTwice_ReturnsSameClientInstance()
    {
        // Arrange
        using var cache = new MongoClientCache();

        // Act
        var first = cache.GetOrCreate(ConnectionStringA);
        var second = cache.GetOrCreate(ConnectionStringA);

        // Assert
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void GetOrCreate_DifferentConnectionStrings_ReturnsDistinctClientInstances()
    {
        // Arrange
        using var cache = new MongoClientCache();

        // Act
        var first = cache.GetOrCreate(ConnectionStringA);
        var second = cache.GetOrCreate(ConnectionStringB);

        // Assert
        second.Should().NotBeSameAs(first);
    }

    [Fact]
    public void GetOrCreate_UsesInjectedFactory_InvokedOnlyOncePerConnectionString()
    {
        // Arrange
        var callCount = 0;
        var expected = new MongoClient(ConnectionStringA);
        using var cache = new MongoClientCache(_ =>
        {
            callCount++;
            return expected;
        });

        // Act
        var first = cache.GetOrCreate(ConnectionStringA);
        var second = cache.GetOrCreate(ConnectionStringA);

        // Assert
        first.Should().BeSameAs(expected);
        second.Should().BeSameAs(expected);
        callCount.Should().Be(1);
    }

    [Fact]
    public void Dispose_WithCreatedClients_DoesNotThrow()
    {
        // Arrange
        var cache = new MongoClientCache();
        cache.GetOrCreate(ConnectionStringA);
        cache.GetOrCreate(ConnectionStringB);

        // Act
        var act = () => cache.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
