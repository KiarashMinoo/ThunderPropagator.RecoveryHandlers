using Microsoft.Extensions.DependencyInjection;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;
using ThunderPropagator.RecoveryHandler.SharedKernel;

namespace ThunderPropagator.RecoveryHandler.MongoDb
{
    public static partial class MongoDbRecoveryHandlerExtensions
    {
        public static IServiceCollection AddMongoDbRecoveryHandler(this IServiceCollection services)
        {
            services.AddSingleton<IRecoveryHandlerFactory, MongoDbRecoveryHandlerFactory>();
            services.AddFeature<MongoDbRecoveryStorageFeature>();


            return services;
        }
    }
}
