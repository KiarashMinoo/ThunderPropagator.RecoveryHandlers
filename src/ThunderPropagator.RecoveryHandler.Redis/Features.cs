using System.ComponentModel;
using ThunderPropagator.Application;

namespace ThunderPropagator.RecoveryHandler.Redis;

/// <summary>
/// Enables support for using Redis as a recovery storage solution,
/// providing fast and reliable data recovery and persistence in case of failures.
/// </summary>
[Description("Supports Redis as a recovery storage solution for fast and reliable data recovery.")]
internal
#if !DEBUG
        sealed
#endif
    class RedisRecoveryStorageFeature : IFeature;
