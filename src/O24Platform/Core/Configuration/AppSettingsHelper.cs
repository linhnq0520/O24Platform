using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Core.Configuration;

/// <summary>
/// The app settings helper class
/// </summary>
public class AppSettingsHelper
{
    /// <summary>
    /// Saves the app settings using the specified configurations
    /// </summary>
    /// <param name="configurations">The configurations</param>
    /// <param name="fileProvider">The file provider</param>
    /// <param name="overwrite">The overwrite</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns>The app settings</returns>
    public static AppSettings SaveAppSettings(
        IList<IConfig> configurations,
        IO24OpenAPIFileProvider fileProvider,
        bool overwrite = true
    )
    {
        ArgumentNullException.ThrowIfNull(configurations);

        AppSettings appSettings = Singleton<AppSettings>.Instance ?? new AppSettings();
        appSettings.Update(configurations);

        Singleton<AppSettings>.Instance = appSettings;
        Singleton<O24OpenAPIConfiguration>.Instance = appSettings.Get<O24OpenAPIConfiguration>();
        return appSettings;
    }

    /// <summary>
    /// Gets the schema name using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns>The schema name</returns>
    public static string GetSchemaName(Type type)
    {
        if (type == null)
        {
            return string.Empty;
        }

        var config = Singleton<O24OpenAPIConfiguration>.Instance;

        if (config == null)
        {
            return string.Empty;
        }

        if (type.Name.Equals("LastProcessedLSN", StringComparison.OrdinalIgnoreCase))
        {
            return config.GetCdcDbSchema();
        }

        var ns = type.Namespace;

        if (ns.StartsWithOrdinalIgnoreCase("O24OpenAPI.DataWarehouse"))
        {
            return config.DWHSchema;
        }

        return string.Empty;
    }
}
