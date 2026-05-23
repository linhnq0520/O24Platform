namespace O24OpenAPI.Core.Configuration;

/// <summary>
/// The config interface
/// </summary>
public interface IConfig
{
    /// <summary>
    /// Gets the value of the name
    /// </summary>
    /// [JsonIgnore]
    string Name => this.GetType().Name;

    /// <summary>
    /// Gets the order
    /// </summary>
    /// <returns>The int</returns>
    int GetOrder() => 1;
}
