namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The 24 open api migration defaults class
/// </summary>
public class O24OpenAPIMigrationDefaults
{
    /// <summary>
    /// Gets the value of the date formats
    /// </summary>
    public static string[] DateFormats { get; } =
        new string[6]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy.MM.dd HH:mm:ss",
            "yyyy/MM/dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss:fffffff",
            "yyyy.MM.dd HH:mm:ss:fffffff",
            "yyyy/MM/dd HH:mm:ss:fffffff",
        };

    /// <summary>
    /// Gets the value of the update migration description
    /// </summary>
    public static string UpdateMigrationDescription { get; } =
        "O24OpenAPI version {0}. Update {1}";

    /// <summary>
    /// Gets the value of the update migration description prefix
    /// </summary>
    public static string UpdateMigrationDescriptionPrefix { get; } =
        "O24OpenAPI version {0}. Update";
}
