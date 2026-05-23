using System.Runtime.CompilerServices;
using LinqToDB.Data;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Configuration;
using O24OpenAPI.Data.DataProviders;

namespace O24OpenAPI.Data;

/// <summary>
/// The data provider manager class
/// </summary>
/// <seealso cref="IDataProviderManager"/>
public class DataProviderManager : IDataProviderManager
{
    private DataConnection _connection;
    private DataConnectionTransaction _transaction;

    /// <summary>
    /// Gets the data provider using the specified data provider type
    /// </summary>
    /// <param name="dataProviderType">The data provider type</param>
    /// <exception cref="O24OpenAPIException"></exception>
    /// <returns>The data provider</returns>
    public static IO24OpenAPIDataProvider GetDataProvider(DataProviderType dataProviderType)
    {
        IO24OpenAPIDataProvider dataProvider;
        switch (dataProviderType)
        {
            case DataProviderType.SqlServer:
                dataProvider = new MsSqlCoreDataProvider();
                break;
            case DataProviderType.Oracle:
                dataProvider = new OracleCoreDataProvider();
                break;
            case DataProviderType.MySql:
                dataProvider = new MySqlCoreDataProvider();
                break;
            case DataProviderType.PostgreSQL:
                dataProvider = new PostgreSqlCoreDataProvider();
                break;
            default:
                DefaultInterpolatedStringHandler interpolatedStringHandler = new(36, 1);
                interpolatedStringHandler.AppendLiteral("Not supported data provider name: '");
                interpolatedStringHandler.AppendFormatted(dataProviderType);
                interpolatedStringHandler.AppendLiteral("'");
                throw new O24OpenAPIException(interpolatedStringHandler.ToStringAndClear());
        }
        return dataProvider;
    }

    /// <summary>
    /// Gets the value of the data provider
    /// </summary>
    public IO24OpenAPIDataProvider DataProvider
    {
        get => GetDataProvider(Singleton<DataConfig>.InstanceRequired.DataProvider);
    }

    public DataConnection CreateDataConnection()
    {
        return _connection ??= GetDataProvider(Singleton<DataConfig>.InstanceRequired.DataProvider)
            .CreateDataConnection();
    }
}
