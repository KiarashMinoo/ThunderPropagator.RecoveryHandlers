using ThunderPropagator.Application.Channels;
using ThunderPropagator.Application.Channels.Snapshots.Recovery;
using ThunderPropagator.Application.LicenseManagers;
using ThunderPropagator.BuildingBlocks.Application.Enums;
using ThunderPropagator.Infrastructure.Channels.Snapshots.Recovery;

namespace ThunderPropagator.RecoveryHandler.MongoDb;

internal sealed class MongoDbRecoveryHandlerFactory(
    IServiceProvider serviceProvider,
    NoneRecoveryHandler noneHandler,
    IFeatureGate featureGate
) : IRecoveryHandlerFactory
{
    public bool CanHandle(IChannel channel) =>
        channel.Metadata.Snapshot.RecoveryStorage == RecoveryStorage.MongoDb;

    public IRecoveryHandler Create(IChannel channel) =>
        featureGate.IsAllowed<MongoDbRecoveryStorageFeature>()
            ? new MongoDbRecoveryHandler(serviceProvider, channel)
            : noneHandler;
}
