namespace O24OpenAPI.Framework.Services.CDC;

public interface ICdcKeyConfigService
{
    Task<string[]> GetKeyColumnsAsync(string tableName);
}
