using System.ComponentModel;
using ThunderPropagator.Application;

namespace ThunderPropagator.RecoveryHandler.Postgresql;

/// <summary>
/// Enables support for using PostgreSQL as a recovery storage solution,
/// ensuring reliable data recovery and persistence in case of failures.
/// </summary>
[Description("Supports PostgreSQL as a recovery storage solution for reliable data recovery and persistence.")]
internal
#if !DEBUG
        sealed
#endif
    class PostgresqlRecoveryStorageFeature : IFeature;
