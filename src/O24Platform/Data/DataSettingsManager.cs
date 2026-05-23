using Newtonsoft.Json;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Configuration;
using System.Text;

namespace O24OpenAPI.Data;

/// <summary>
/// The data settings manager class
/// </summary>
public class DataSettingsManager
{
    /// <summary>
    /// The database is installed
    /// </summary>
    private static bool? _databaseIsInstalled;

    /// <summary>
    /// Loads the data settings from json file using the specified data
    /// </summary>
    /// <param name="data">The data</param>
    /// <returns>The data config</returns>
    protected static DataConfig LoadDataSettingsFromJsonFile(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return (DataConfig)null;
        }

        var data1 = JsonConvert.DeserializeAnonymousType(
            data,
            new
            {
                DataConnectionString = "",
                DataProvider = DataProviderType.SqlServer,
                SQLCommandTimeout = "",
            }
        );
        int result;
        return new DataConfig()
        {
            ConnectionString = data1.DataConnectionString,
            DataProvider = data1.DataProvider,
            SQLCommandTimeout = int.TryParse(data1.SQLCommandTimeout, out result)
                ? new int?(result)
                : new int?(),
        };
    }

    /// <summary>
    /// Loads the settings using the specified file provider
    /// </summary>
    /// <param name="fileProvider">The file provider</param>
    /// <returns>The data config</returns>
    public static DataConfig LoadSettings(IO24OpenAPIFileProvider fileProvider = null)
    {
        if (Singleton<DataConfig>.Instance != null)
        {
            return Singleton<DataConfig>.Instance;
        }

        fileProvider ??= CommonHelper.DefaultFileProvider;

        string str = fileProvider.MapPath(O24OpenAPIDataSettingsDefaults.FilePath);
        if (fileProvider.FileExists(str))
        {
            DataConfig dataSettings = LoadDataSettingsFromJsonFile(
                fileProvider.ReadAllText(str, Encoding.UTF8)
            );
            fileProvider.DeleteFile(str);
            SaveSettings(dataSettings, fileProvider);
            Singleton<DataConfig>.Instance = dataSettings;
        }
        else
        {
            Singleton<DataConfig>.Instance = Singleton<AppSettings>.Instance.Get<DataConfig>();
        }
        return Singleton<DataConfig>.Instance;
    }

    /// <summary>
    /// Ises the database installed
    /// </summary>
    /// <returns>The bool</returns>
    public static bool IsDatabaseInstalled()
    {
        _databaseIsInstalled.GetValueOrDefault();
        if (!_databaseIsInstalled.HasValue)
        {
            _databaseIsInstalled = new bool?(
                !string.IsNullOrEmpty(LoadSettings()?.ConnectionString)
            );
        }

        return Singleton<O24OpenAPIConfiguration>.Instance.RunMigration
            && _databaseIsInstalled.Value;
    }

    /// <summary>
    /// Saves the settings using the specified data settings
    /// </summary>
    /// <param name="dataSettings">The data settings</param>
    /// <param name="fileProvider">The file provider</param>
    public static void SaveSettings(
        DataConfig dataSettings,
        IO24OpenAPIFileProvider fileProvider
    )
    {
        Singleton<DataConfig>.Instance = (DataConfig)null;
        AppSettingsHelper.SaveAppSettings(
            (IList<IConfig>)new List<IConfig>() { (IConfig)dataSettings },
            fileProvider
        );
    }

    /// <summary>
    /// Gets the sql command timeout
    /// </summary>
    /// <returns>The int</returns>
    public static int GetSqlCommandTimeout()
    {
        return (
            (int?)DataSettingsManager.LoadSettings()?.SQLCommandTimeout
        ).GetValueOrDefault();
    }
}
