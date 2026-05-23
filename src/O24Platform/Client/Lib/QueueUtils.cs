namespace O24OpenAPI.Client.Lib;

/// <summary>
/// The queue utils class
/// </summary>
public static class QueueUtils
{
    /// <summary>
    /// Gets the event queue name using the specified service code
    /// </summary>
    /// <param name="serviceCode">The service code</param>
    /// <returns>The string</returns>
    public static string GetEventQueueName(string serviceCode)
    {
        return $"event_queue:queue_{serviceCode}";
    }

    /// <summary>
    /// Gets the command queue name using the specified service code
    /// </summary>
    /// <param name="serviceCode">The service code</param>
    /// <returns>The string</returns>
    public static string GetCommandQueueName(string serviceCode)
    {
        return $"command_queue:queue_{serviceCode}";
    }
}
