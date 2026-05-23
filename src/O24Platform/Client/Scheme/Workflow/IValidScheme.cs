namespace O24OpenAPI.Client.Scheme.Workflow;

/// <summary>
/// The valid scheme interface
/// </summary>
public interface IValidScheme
{
    /// <summary>
    /// Ises the valid using the specified error
    /// </summary>
    /// <param name="error">The error</param>
    /// <returns>The bool</returns>
    bool IsValid(out string error);
}
