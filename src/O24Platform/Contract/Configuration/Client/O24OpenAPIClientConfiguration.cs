using System.Text.Json;
using O24OpenAPI.Core.Configuration;

namespace O24OpenAPI.Contracts.Configuration.Client;

public class O24OpenAPIClientConfiguration
{
    /// <summary>
    /// Gets or sets the value of the wfo grpc url
    /// </summary>
    public string WFOGrpcURL { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the your grpc url
    /// </summary>
    public string YourGrpcURL { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the your service id
    /// </summary>
    public string YourServiceID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the your instance id
    /// </summary>
    public string YourInstanceID { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the value of the environment variables
    /// </summary>
    public List<EnvironmentVariable> EnvironmentVariables { get; set; } = [];

    /// <summary>
    /// Gets or sets the value of the o 24 open api grpc token
    /// </summary>
    public string O24OpenAPIGrpcToken { get; set; } = string.Empty;

    /// <summary>
    /// Clones this instance
    /// </summary>
    /// <returns>The 24 open api configuration</returns>
    public O24OpenAPIClientConfiguration Clone()
    {
        var json = JsonSerializer.Serialize(this);
        var clone =
            JsonSerializer.Deserialize<O24OpenAPIClientConfiguration>(json)
            ?? throw new InvalidOperationException("Clone O24OpenAPIClientConfiguration failed.");
        return clone;
    }

    public O24OpenAPIClientConfiguration() { }

    public O24OpenAPIClientConfiguration(O24OpenAPIConfiguration o24OpenAPIConfiguration)
    {
        WFOGrpcURL = o24OpenAPIConfiguration.WFOGrpcURL;
        YourGrpcURL = o24OpenAPIConfiguration.YourGrpcURL;
        YourServiceID = o24OpenAPIConfiguration.YourServiceID;
        YourInstanceID = o24OpenAPIConfiguration.YourInstanceID;
        O24OpenAPIGrpcToken = o24OpenAPIConfiguration.O24OpenAPIGrpcToken;
    }
}
