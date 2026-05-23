namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The store procedure error model class
/// </summary>
public class StoreProcedureErrorModel
{
    /// <summary>
    /// Gets or sets the value of the error message
    /// </summary>
    public string error_message { get; set; }

    /// <summary>
    /// Gets or sets the value of the error code
    /// </summary>
    public string error_code { get; set; }
}
