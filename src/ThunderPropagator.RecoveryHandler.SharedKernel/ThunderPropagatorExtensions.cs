using Microsoft.Extensions.DependencyInjection;
using ThunderPropagator.Application.Features;

namespace ThunderPropagator.RecoveryHandler.SharedKernel
{
    public static partial class ThunderPropagatorExtensions
    {
        internal static IServiceCollection AddFeature<TFeature>(this IServiceCollection services) where TFeature : class, IFeature
        {
            Infrastructure.Extensions.ThunderPropagatorExtensions.AddFeature<TFeature>(services);
            return services;
        }
    }
}
