using Microsoft.Extensions.DependencyInjection;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;
using ThunderPropagator.RecoveryHandler.SharedKernel;

namespace ThunderPropagator.RecoveryHandler.MongoDb
{
    public static partial class MongoDbRecoveryHandlerExtensions
    {
        public static IServiceCollection AddMongoDbRecoveryHandler(this IServiceCollection services)
        {
            // Singleton: shares one MongoClient per distinct connection string across every
            // channel's MongoDbRecoveryHandler, instead of creating (and never disposing) one per channel.
            services.AddSingleton<MongoClientCache>();
            services.AddSingleton<IRecoveryHandlerFactory, MongoDbRecoveryHandlerFactory>();
            services.AddFeature<MongoDbRecoveryStorageFeature>();

            return services;
        }
    }
}
