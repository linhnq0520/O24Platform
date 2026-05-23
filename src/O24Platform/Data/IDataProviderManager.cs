namespace O24OpenAPI.Data;

/// <summary>
/// The data provider manager interface
/// </summary>
public interface IDataProviderManager
{
    /// <summary>
    /// Gets the value of the data provider
    /// </summary>
    IO24OpenAPIDataProvider DataProvider { get; }
}
