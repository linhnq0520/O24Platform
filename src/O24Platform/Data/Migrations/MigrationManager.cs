using System.Reflection;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Attributes;
using O24OpenAPI.Data.Configuration;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The migration manager class
/// </summary>
/// <seealso cref="IMigrationManager"/>
public class MigrationManager(
    IServiceProvider serviceProvider,
    IFilteringMigrationSource filteringMigrationSource,
    IMigrationRunner migrationRunner,
    IMigrationRunnerConventions migrationRunnerConventions
) : IMigrationManager
{
    /// <summary>
    /// The filtering migration source
    /// </summary>
    private readonly IFilteringMigrationSource _filteringMigrationSource = filteringMigrationSource;

    /// <summary>
    /// The migration runner
    /// </summary>
    private readonly IMigrationRunner _migrationRunner = migrationRunner;

    /// <summary>
    /// The migration runner conventions
    /// </summary>
    private readonly IMigrationRunnerConventions _migrationRunnerConventions =
        migrationRunnerConventions;

    /// <summary>
    /// The create scope
    /// </summary>
    private readonly Lazy<IVersionLoader> _versionLoader = new(() =>
        EngineContext.Current.Resolve<IVersionLoader>(serviceProvider.CreateScope())
    );

    /// <summary>
    /// Gets the up migrations using the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly</param>
    /// <param name="migrationProcessType">The migration process type</param>
    /// <param name="environmentType">The environment type</param>
    /// <returns>An enumerable of i migration info</returns>
    protected virtual IEnumerable<IMigrationInfo> GetUpMigrations(
        Assembly assembly,
        MigrationProcessType migrationProcessType = MigrationProcessType.NoMatter,
        EnvironmentType environmentType = EnvironmentType.Dev
    )
    {
        var migrations = _filteringMigrationSource.GetMigrations(t =>
        {
            var attributes = t.GetCustomAttributes<Attribute>().ToArray();
            var customAttribute = attributes
                .OfType<O24OpenAPIMigrationAttribute>()
                .FirstOrDefault();
            var environmentAttribute = attributes.OfType<EnvironmentAttribute>().FirstOrDefault();

            var databaseTypeAttribute = attributes.OfType<DatabaseTypeAttribute>().FirstOrDefault();
            var dbConfig = Singleton<DataConfig>.Instance;
            bool isDatabaseTypeMatch = true;
            if (databaseTypeAttribute is not null && dbConfig is not null)
            {
                if (!databaseTypeAttribute.DatabaseTypes.Contains(dbConfig.DataProvider))
                {
                    isDatabaseTypeMatch = false;
                }
            }

            if (customAttribute == null || environmentAttribute == null)
            {
                return false;
            }

            bool isMigrationNotApplied = !_versionLoader.Value.VersionInfo.HasAppliedMigration(
                customAttribute.Version
            );
            bool isProcessTypeMatch =
                customAttribute.TargetMigrationProcess == MigrationProcessType.NoMatter
                || migrationProcessType == MigrationProcessType.NoMatter
                || migrationProcessType == customAttribute.TargetMigrationProcess;
            bool isAssemblyMatch = assembly == null || t.Assembly == assembly;
            bool isEnvironmentMatch =
                environmentAttribute != null
                && environmentAttribute.EnvironmentTypes.Any(attr =>
                    attr == environmentType || attr == EnvironmentType.All
                );

            return isMigrationNotApplied
                && isProcessTypeMatch
                && isAssemblyMatch
                && isEnvironmentMatch
                && isDatabaseTypeMatch;
        });

        if (migrations == null)
        {
            return Enumerable.Empty<IMigrationInfo>();
        }

        return migrations
            .Select(m => _migrationRunnerConventions.GetMigrationInfoForMigration(m))
            .OrderBy(migration => migration.Version);
    }

    /// <summary>
    /// Gets the down migrations using the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly</param>
    /// <param name="migrationProcessType">The migration process type</param>
    /// <returns>An enumerable of i migration info</returns>
    protected virtual IEnumerable<IMigrationInfo> GetDownMigrations(
        Assembly assembly,
        MigrationProcessType migrationProcessType = MigrationProcessType.NoMatter
    )
    {
        return (
            this._filteringMigrationSource.GetMigrations(t =>
            {
                O24OpenAPIMigrationAttribute customAttribute =
                    t.GetCustomAttribute<O24OpenAPIMigrationAttribute>();
                return customAttribute != null
                    && this._versionLoader.Value.VersionInfo.HasAppliedMigration(
                        customAttribute.Version
                    )
                    && (
                        customAttribute.TargetMigrationProcess == MigrationProcessType.NoMatter
                        || migrationProcessType == MigrationProcessType.NoMatter
                        || migrationProcessType == customAttribute.TargetMigrationProcess
                    )
                    && (assembly == null || t.Assembly == assembly);
            }) ?? []
        ).Select(m => this._migrationRunnerConventions.GetMigrationInfoForMigration(m)).OrderBy(migration => migration.Version);
    }

    /// <summary>
    /// Applies the up migrations using the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly</param>
    /// <param name="migrationProcessType">The migration process type</param>
    /// <param name="environmentType">The environment type</param>
    public void ApplyUpMigrations(
        Assembly assembly,
        MigrationProcessType migrationProcessType = MigrationProcessType.Installation,
        EnvironmentType environmentType = EnvironmentType.Dev
    )
    {
        ArgumentNullException.ThrowIfNull(assembly);
        foreach (
            IMigrationInfo upMigration in this.GetUpMigrations(
                assembly,
                migrationProcessType,
                environmentType
            )
        )
        {
            this._migrationRunner.Up(upMigration.Migration);
            if (
                string.IsNullOrEmpty(upMigration.Description)
                || !upMigration.Description.StartsWith(
                    string.Format(O24OpenAPIMigrationDefaults.UpdateMigrationDescriptionPrefix, "1")
                )
            )
            {
                this._versionLoader.Value.UpdateVersionInfo(
                    upMigration.Version,
                    upMigration.Description ?? upMigration.Migration.GetType().Name
                );
            }
        }
    }

    /// <summary>
    /// Applies the down migrations using the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly</param>
    public void ApplyDownMigrations(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        foreach (
            IMigrationInfo migrationInfo in this.GetDownMigrations(assembly)
                .Reverse<IMigrationInfo>()
        )
        {
            this._migrationRunner.Down(migrationInfo.Migration);
            this._versionLoader.Value.DeleteVersion(migrationInfo.Version);
        }
    }
}
