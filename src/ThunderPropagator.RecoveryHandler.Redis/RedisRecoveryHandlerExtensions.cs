using Microsoft.Extensions.DependencyInjection;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;
using ThunderPropagator.RecoveryHandler.SharedKernel;

namespace ThunderPropagator.RecoveryHandler.Redis
{
    public static partial class RedisRecoveryHandlerExtensions
    {
        public static IServiceCollection AddRedisRecoveryHandler(this IServiceCollection services)
        {
            // Singleton: shares one IConnectionMultiplexer per distinct Redis connection string
            // across every channel's RedisRecoveryHandler, instead of connecting once per channel.
            services.AddSingleton<RedisConnectionMultiplexerCache>();
            services.AddSingleton<IRecoveryHandlerFactory, RedisRecoveryHandlerFactory>();
            services.AddFeature<RedisRecoveryStorageFeature>();

            return services;
        }
    }
}
