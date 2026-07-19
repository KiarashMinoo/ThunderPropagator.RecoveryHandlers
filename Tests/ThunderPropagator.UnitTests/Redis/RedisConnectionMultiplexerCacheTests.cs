using FluentAssertions;
using NSubstitute;
using StackExchange.Redis;
using ThunderPropagator.RecoveryHandler.Redis;

namespace ThunderPropagator.UnitTests.Redis;

public class RedisConnectionMultiplexerCacheTests
{
    [Fact]
    public async Task GetOrCreateAsync_SameConnectionStringCalledTwice_ReturnsSameInstanceAndInvokesFactoryOnce()
    {
        // Arrange
        var callCount = 0;
        var expected = Substitute.For<IConnectionMultiplexer>();
        var cache = new RedisConnectionMultiplexerCache(_ =>
        {
            callCount++;
            return Task.FromResult(expected);
        });

        // Act
        var first = await cache.GetOrCreateAsync("redis-a");
        var second = await cache.GetOrCreateAsync("redis-a");

        // Assert
        first.Should().BeSameAs(expected);
        second.Should().BeSameAs(first);
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOrCreateAsync_DifferentConnectionStrings_ReturnsDistinctInstances()
    {
        // Arrange
        var cache = new RedisConnectionMultiplexerCache(_ => Task.FromResult(Substitute.For<IConnectionMultiplexer>()));

        // Act
        var first = await cache.GetOrCreateAsync("redis-a");
        var second = await cache.GetOrCreateAsync("redis-b");

        // Assert
        first.Should().NotBeSameAs(second);
    }

    [Fact]
    public async Task GetOrCreateAsync_FirstAttemptFails_DoesNotPermanentlyCacheTheFailure()
    {
        // Arrange
        var attempt = 0;
        var expected = Substitute.For<IConnectionMultiplexer>();
        var cache = new RedisConnectionMultiplexerCache(_ =>
        {
            attempt++;
            return attempt == 1
                ? Task.FromException<IConnectionMultiplexer>(new InvalidOperationException("connect failed"))
                : Task.FromResult(expected);
        });

        // Act
        var firstAttempt = async () => await cache.GetOrCreateAsync("redis-a");
        await firstAttempt.Should().ThrowAsync<InvalidOperationException>();

        var result = await cache.GetOrCreateAsync("redis-a");

        // Assert — a transient connect failure must not permanently poison the cache entry;
        // the next call for the same connection string retries instead of rethrowing forever.
        result.Should().BeSameAs(expected);
        attempt.Should().Be(2);
    }

    [Fact]
    public async Task DisposeAsync_WithACreatedMultiplexer_DisposesIt()
    {
        // Arrange
        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        var cache = new RedisConnectionMultiplexerCache(_ => Task.FromResult(multiplexer));
        await cache.GetOrCreateAsync("redis-a");

        // Act
        await cache.DisposeAsync();

        // Assert
        await multiplexer.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_WithNoMultiplexersCreated_DoesNotThrow()
    {
        // Arrange
        var cache = new RedisConnectionMultiplexerCache(_ => Task.FromResult(Substitute.For<IConnectionMultiplexer>()));

        // Act
        var act = async () => await cache.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }
}
