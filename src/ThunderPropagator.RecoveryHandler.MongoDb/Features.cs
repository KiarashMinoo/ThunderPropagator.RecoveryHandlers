using System.ComponentModel;
using ThunderPropagator.Application;
using ThunderPropagator.Application.Features;

namespace ThunderPropagator.RecoveryHandler.MongoDb;

/// <summary>
/// Enables support for using MongoDB as a recovery storage solution,
/// ensuring reliable data recovery and persistence in case of failures.
/// </summary>
[Description("Supports MongoDB as a recovery storage solution for reliable data recovery and persistence.")]
internal
#if !DEBUG
        sealed
#endif
    class MongoDbRecoveryStorageFeature : IFeature;
