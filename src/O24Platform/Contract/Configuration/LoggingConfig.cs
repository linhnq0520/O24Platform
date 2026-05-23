namespace O24OpenAPI.Contracts.Configuration;

public sealed class LoggingConfig
{
    public const string SectionName = "Logging";

    public HashSet<string> PathNoLogRestApi { get; set; } = [];
}
