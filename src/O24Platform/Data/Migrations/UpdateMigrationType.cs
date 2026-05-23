namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The update migration type enum
/// </summary>
public enum UpdateMigrationType
{
    /// <summary>
    /// The data update migration type
    /// </summary>
    Data = 5,
    /// <summary>
    /// The localization update migration type
    /// </summary>
    Localization = 10, // 0x0000000A
    /// <summary>
    /// The settings update migration type
    /// </summary>
    Settings = 15, // 0x0000000F
}
