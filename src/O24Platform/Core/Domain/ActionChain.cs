namespace O24OpenAPI.Core.Domain;

/// <summary>
/// The action chain class
/// </summary>
public class ActionChain
{
    /// <summary>
    /// Gets or sets the value of the action
    /// </summary>
    public string Action { get; set; } = "U";

    /// <summary>
    /// Gets or sets the value of the update field
    /// </summary>
    public string? UpdateField { get; set; }

    /// <summary>
    /// Gets or sets the value of the update fields
    /// </summary>
    public List<string>? UpdateFields { get; set; }

    /// <summary>
    /// Gets or sets the value of the update value
    /// </summary>
    public object? UpdateValue { get; set; }
}
