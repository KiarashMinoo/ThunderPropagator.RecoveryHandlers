using System.Collections.Concurrent;
using MongoDB.Driver;

namespace ThunderPropagator.RecoveryHandler.MongoDb
{
    /// <summary>
    /// Caches one <see cref="MongoClient"/> per distinct connection string and shares it across
    /// every <see cref="MongoDbRecoveryHandler"/> that targets the same cluster.
    /// </summary>
    /// <remarks>
    /// The MongoDB .NET driver documents <see cref="MongoClient"/> as thread-safe and intended to
    /// be created once per cluster and reused for the lifetime of the application — each instance
    /// owns its own connection pool and background cluster monitor, so creating one per channel
    /// (as the original implementation did) wastes sockets and monitoring threads whenever more
    /// than one channel points at the same MongoDB deployment. It also meant every handler's
    /// client was never disposed. This cache owns disposal centrally: it is registered as a
    /// singleton (see <see cref="MongoDbRecoveryHandlerExtensions.AddMongoDbRecoveryHandler"/>)
    /// and disposes every cached client when the root <see cref="IServiceProvider"/> is disposed.
    /// </remarks>
    internal sealed class MongoClientCache : IDisposable
    {
        private readonly Func<string, MongoClient> _createClient;
        private readonly ConcurrentDictionary<string, MongoClient> _clients = new();

        public MongoClientCache() : this(connectionString => new MongoClient(MongoUrl.Create(connectionString)))
        {
        }

        /// <summary>
        /// Test-only constructor allowing client creation to be substituted for unit tests.
        /// </summary>
        internal MongoClientCache(Func<string, MongoClient> createClient)
        {
            _createClient = createClient;
        }

        public MongoClient GetOrCreate(string connectionString) =>
            _clients.GetOrAdd(connectionString, _createClient);

        public void Dispose()
        {
            foreach (var client in _clients.Values)
                client.Dispose();

            _clients.Clear();
        }
    }
}
