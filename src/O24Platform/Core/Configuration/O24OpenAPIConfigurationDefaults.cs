namespace O24OpenAPI.Core.Configuration;

/// <summary>
/// The 24 open api configuration defaults class
/// </summary>
public static class O24OpenAPIConfigurationDefaults
{
    /// <summary>
    /// Gets the value of the app settings file path
    /// </summary>
    public static string AppSettingsFilePath => "App_Data/appsettings.json";

    /// <summary>
    /// Gets the value of the app settings environment file path
    /// </summary>
    public static string AppSettingsEnvironmentFilePath => "App_Data/appsettings.{0}.json";
}
