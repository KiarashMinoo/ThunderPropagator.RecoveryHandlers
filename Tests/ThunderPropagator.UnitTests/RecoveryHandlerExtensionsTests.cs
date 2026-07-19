using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;
using ThunderPropagator.RecoveryHandler.MongoDb;
using ThunderPropagator.RecoveryHandler.Postgresql;
using ThunderPropagator.RecoveryHandler.Redis;

namespace ThunderPropagator.UnitTests;

public class RecoveryHandlerExtensionsTests
{
    [Fact]
    public void AddMongoDbRecoveryHandler_EmptyServices_RegistersFactoryAndReturnsServices()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        var result = services.AddMongoDbRecoveryHandler();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IRecoveryHandlerFactory) &&
            descriptor.ImplementationType == typeof(MongoDbRecoveryHandlerFactory) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPostgresqlRecoveryHandler_EmptyServices_RegistersFactoryAndReturnsServices()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        var result = services.AddPostgresqlRecoveryHandler();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IRecoveryHandlerFactory) &&
            descriptor.ImplementationType == typeof(PostgresqlRecoveryHandlerFactory) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddRedisRecoveryHandler_EmptyServices_RegistersFactoryAndReturnsServices()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        var result = services.AddRedisRecoveryHandler();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IRecoveryHandlerFactory) &&
            descriptor.ImplementationType == typeof(RedisRecoveryHandlerFactory) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
    }
}
