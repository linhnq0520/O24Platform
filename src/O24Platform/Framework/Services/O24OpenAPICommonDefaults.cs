namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The 24 open api common defaults class
/// </summary>
public static class O24OpenAPICommonDefaults
{
    /// <summary>
    /// Gets the value of the keep alive path
    /// </summary>
    public static string KeepAlivePath => "keepalive/index";

    /// <summary>
    /// Gets the value of the default language culture
    /// </summary>
    public static string DefaultLanguageCulture => "en-US";

    /// <summary>
    /// Gets the value of the remove old data stored procedure name
    /// </summary>
    public static string RemoveOldDataStoredProcedureName => "FW_REMOVE_TRANSACTION";

    /// <summary>
    /// Gets the value of the list gl entries not posted sp name
    /// </summary>
    public static string ListGLEntriesNotPostedSPName => "FW_LIST_GLENTRIES_BY_TRANSACTION_NUMBER";

    /// <summary>
    /// Gets the value of the remove log stored procedure name
    /// </summary>
    public static string RemoveLogStoredProcedureName => "FW_CLEAR_LOG";
}
