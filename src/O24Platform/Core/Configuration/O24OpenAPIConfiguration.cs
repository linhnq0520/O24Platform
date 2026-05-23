namespace O24OpenAPI.Core.Configuration;

public class O24OpenAPIConfiguration : IConfig
{
    public string Environment { get; set; } = "Dev";
    public bool RunMigration { get; set; } = true;
    public string AssemblyMigration { get; set; } = "";
    public string YourServiceID { get; set; } = "MS1";
    public string YourInstanceID { get; set; } = Guid.NewGuid().ToString("N");
    public string YourDatabase { get; set; } = "";
    public string YourSchema { get; set; } = "dbo";
    public string YourCDCSchema { get; set; } = "cdc";
    public bool ConnectToWFO { get; set; } = true;
    public string O24OpenAPIGrpcToken { get; set; } = "";
    public string WFOHttpURL { get; set; } = "";
    public required string WFOGrpcURL { get; set; }
    public required string YourGrpcURL { get; set; }
    public string? URLSMS { get; set; }
    public bool LogEventMessage { get; set; }
    public HashSet<string> DataWarehouseEntities { get; set; } = [];
    public string? DWHSchema { get; set; }
    public string CreateDatabaseScriptPath { get; set; } = "App_Data/CreateDatabase.sql";
    public string CreateSchemaScriptPath { get; set; } = "App_Data/CreateSchema.sql";
    public string EnableCDCScriptPath { get; set; } = "App_Data/EnableCDC.sql";
    public string OpenAPIURI { get; set; } = "";
    public List<string> FanoutExchanges { get; set; } = [];
    public string FileLogPath { get; set; } = "App_Data/LogFile.txt";
    public bool AutoDeleteCommandQueue { get; set; } = true;
    public bool AutoDeleteEventQueue { get; set; } = false;
    public string OpenAPICMSURI { get; set; } = "";
    public string AllowedCorsOrigins { get; set; } = "";
    public HashSet<string> ProxyAuthenticationPaths { get; set; } =
        ["/api/chat", "/api/proxy", "/api/w4s", "/w4s/api"];
    public HashSet<string> OpenApiPaths { get; set; } =
        ["/api/v1/openapi/gateway", "/api/v1/oauth/token"];
    public bool JobExecutionHistoryEnabled { get; set; } = true;

    public string GetCdcDbSchema()
    {
        return $"{YourDatabase}.{YourCDCSchema}";
    }

    public string GetDbSchema()
    {
        return $"{YourDatabase}.{YourSchema}";
    }
}
