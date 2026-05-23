namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The response class
/// </summary>
public class Response
{
    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public EnumResponseStatus status { get; set; } = EnumResponseStatus.SUCCESS;

    /// <summary>
    /// Gets or sets the value of the error message
    /// </summary>
    public string error_message { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    public Dictionary<string, object> data { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// The enum response status enum
    /// </summary>
    public enum EnumResponseStatus
    {
        /// <summary>
        /// The success enum response status
        /// </summary>
        SUCCESS,

        /// <summary>
        /// The error enum response status
        /// </summary>
        ERROR,
    }
}
