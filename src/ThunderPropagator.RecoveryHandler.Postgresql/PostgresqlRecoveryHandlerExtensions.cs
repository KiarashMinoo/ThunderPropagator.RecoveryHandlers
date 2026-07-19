using Microsoft.Extensions.DependencyInjection;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;
using ThunderPropagator.RecoveryHandler.SharedKernel;

namespace ThunderPropagator.RecoveryHandler.Postgresql
{
    public static partial class PostgresqlRecoveryHandlerExtensions
    {
        public static IServiceCollection AddPostgresqlRecoveryHandler(this IServiceCollection services)
        {
            services.AddSingleton<IRecoveryHandlerFactory, PostgresqlRecoveryHandlerFactory>();
            services.AddFeature<PostgresqlRecoveryStorageFeature>();

            return services;
        }
    }
}
