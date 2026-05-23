using System.Reflection;
using O24OpenAPI.Core.Attributes;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The migration manager interface
/// </summary>
public interface IMigrationManager
{
    /// <summary>
    /// Applies the up migrations using the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly</param>
    /// <param name="migrationProcessType">The migration process type</param>
    /// <param name="environment">The environment</param>
    void ApplyUpMigrations(
        Assembly assembly,
        MigrationProcessType migrationProcessType = MigrationProcessType.Installation,
        EnvironmentType environment = EnvironmentType.Dev
    );

    /// <summary>
    /// Applies the down migrations using the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly</param>
    void ApplyDownMigrations(Assembly assembly);
}
