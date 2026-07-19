using System.Collections.Concurrent;
using StackExchange.Redis;

namespace ThunderPropagator.RecoveryHandler.Redis
{
    /// <summary>
    /// Caches one <see cref="IConnectionMultiplexer"/> per distinct Redis connection string and
    /// shares it across every <see cref="RedisRecoveryHandler"/> that targets the same server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>ConnectionMultiplexer</c> instances are expensive — each one owns its own set of
    /// physical sockets and a background connection-management thread — and StackExchange.Redis'
    /// own guidance is to create a single multiplexer per distinct configuration and reuse it for
    /// the lifetime of the process. Without this cache, every channel configured to use Redis
    /// recovery storage would open its own multiplexer even when several channels point at the
    /// same Redis server, multiplying socket and thread usage for no benefit.
    /// </para>
    /// <para>
    /// Registered as a singleton (see <see cref="RedisRecoveryHandlerExtensions.AddRedisRecoveryHandler"/>),
    /// so it is disposed exactly once, when the root <see cref="IServiceProvider"/> is disposed at
    /// application shutdown.
    /// </para>
    /// </remarks>
    internal sealed class RedisConnectionMultiplexerCache : IAsyncDisposable
    {
        private readonly Func<string, Task<IConnectionMultiplexer>> _connect;
        private readonly ConcurrentDictionary<string, Lazy<Task<IConnectionMultiplexer>>> _multiplexers = new();

        public RedisConnectionMultiplexerCache()
            : this(async connectionString => await ConnectionMultiplexer.ConnectAsync(connectionString).ConfigureAwait(false))
        {
        }

        /// <summary>
        /// Test-only constructor allowing the actual connect operation to be substituted so cache
        /// behaviour (dedup, retry-after-failure) can be verified without a live Redis server.
        /// </summary>
        internal RedisConnectionMultiplexerCache(Func<string, Task<IConnectionMultiplexer>> connect)
        {
            _connect = connect;
        }

        /// <summary>
        /// Returns the shared <see cref="IConnectionMultiplexer"/> for <paramref name="connectionString"/>,
        /// connecting it on first use. Concurrent callers for the same connection string are
        /// coalesced onto a single in-flight connect operation. If a previous attempt for this
        /// connection string faulted, the failure is not cached — the next call retries.
        /// </summary>
        public async Task<IConnectionMultiplexer> GetOrCreateAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                var lazy = _multiplexers.GetOrAdd(connectionString,
                    cs => new Lazy<Task<IConnectionMultiplexer>>(() => _connect(cs)));

                try
                {
                    return await lazy.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch when (lazy.Value.IsFaulted || lazy.Value.IsCanceled)
                {
                    // Don't let a transient connect failure permanently poison the cache entry —
                    // remove it (only if it's still the same faulted Lazy) so the next call retries.
                    _multiplexers.TryRemove(new KeyValuePair<string, Lazy<Task<IConnectionMultiplexer>>>(connectionString, lazy));

                    if (cancellationToken.IsCancellationRequested)
                        throw;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var lazy in _multiplexers.Values)
            {
                if (!lazy.IsValueCreated || !lazy.Value.IsCompletedSuccessfully)
                    continue;

                try
                {
                    var multiplexer = await lazy.Value.ConfigureAwait(false);
                    await multiplexer.DisposeAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Best-effort cleanup during shutdown — one failed disposal must not stop the others.
                }
            }

            _multiplexers.Clear();
        }
    }
}
