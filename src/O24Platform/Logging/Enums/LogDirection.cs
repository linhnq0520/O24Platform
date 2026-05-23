namespace O24OpenAPI.Logging.Enums;

/// <summary>
/// Defines the direction of the communication for logging purposes.
/// </summary>
public enum LogDirection
{
    /// <summary>
    /// An incoming call to this service.
    /// </summary>
    In,

    /// <summary>
    /// An outgoing call from this service to another.
    /// </summary>
    Out,
}
