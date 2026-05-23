namespace O24OpenAPI.Core.Domain.Logging;

/// <summary>
/// The log level enum
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// The debug log level
    /// </summary>
    Debug = 10, // 0x0000000A

    /// <summary>
    /// The information log level
    /// </summary>
    Information = 20, // 0x00000014

    /// <summary>
    /// The warning log level
    /// </summary>
    Warning = 30, // 0x0000001E

    /// <summary>
    /// The error log level
    /// </summary>
    Error = 40, // 0x00000028

    /// <summary>
    /// The fatal log level
    /// </summary>
    Fatal = 50, // 0x00000032
}
