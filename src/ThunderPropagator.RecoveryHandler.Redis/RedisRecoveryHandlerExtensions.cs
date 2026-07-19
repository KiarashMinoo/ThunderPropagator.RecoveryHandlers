using Microsoft.Extensions.DependencyInjection;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;
using ThunderPropagator.RecoveryHandler.SharedKernel;

namespace ThunderPropagator.RecoveryHandler.Redis
{
    public static partial class RedisRecoveryHandlerExtensions
    {
        public static IServiceCollection AddRedisRecoveryHandler(this IServiceCollection services)
        {
            services.AddSingleton<IRecoveryHandlerFactory, RedisRecoveryHandlerFactory>();
            services.AddFeature<RedisRecoveryStorageFeature>();

            return services;
        }
    }
}
