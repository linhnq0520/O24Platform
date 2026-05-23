using O24OpenAPI.Core.ComponentModel;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Data.Mapping;

/// <summary>
/// The name compatibility manager class
/// </summary>
public static class NameCompatibilityManager
{
    /// <summary>
    /// The type
    /// </summary>
    private static readonly Dictionary<Type, string> _tableNames = new Dictionary<Type, string>();

    /// <summary>
    /// The type
    /// </summary>
    private static readonly Dictionary<(Type, string), string> _columnName =
        new Dictionary<(Type, string), string>();

    /// <summary>
    /// The type
    /// </summary>
    private static readonly IList<Type> _loadedFor = (IList<Type>)new List<Type>();

    /// <summary>
    /// The is initialized
    /// </summary>
    private static bool _isInitialized;

    /// <summary>
    /// The reader writer lock slim
    /// </summary>
    private static readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

    /// <summary>
    /// Initializes
    /// </summary>
    private static void Initialize()
    {
        using (
            new ReaderWriteLockDisposable(NameCompatibilityManager._locker, (ReaderWriteLockType)1)
        )
        {
            if (NameCompatibilityManager._isInitialized)
            {
                return;
            }

            IEnumerable<Type> classesOfType =
                Singleton<ITypeFinder>.Instance.FindClassesOfType<INameCompatibility>(true);
            List<INameCompatibility> source =
                (
                    classesOfType != null
                        ? classesOfType
                            .Select(type =>
                                EngineContext.Current.ResolveUnregistered(type)
                                as INameCompatibility
                            )
                            .ToList()
                        : null
                ) ?? new List<INameCompatibility>();
            source.AddRange(
                NameCompatibilityManager.AdditionalNameCompatibilities.Select(type =>
                    EngineContext.Current.ResolveUnregistered(type) as INameCompatibility
                )
            );

            foreach (INameCompatibility nameCompatibility in source.Distinct())
            {
                if (!NameCompatibilityManager._loadedFor.Contains(nameCompatibility.GetType()))
                {
                    NameCompatibilityManager._loadedFor.Add(nameCompatibility.GetType());

                    foreach (
                        var tableName in nameCompatibility.TableNames.Where(tableName =>
                            !NameCompatibilityManager._tableNames.ContainsKey(tableName.Key)
                        )
                    )
                    {
                        string str2 = tableName.Value;
                        NameCompatibilityManager._tableNames.Add(tableName.Key, str2);
                    }

                    foreach (
                        var columnName in nameCompatibility.ColumnName.Where(columnName =>
                            !NameCompatibilityManager._columnName.ContainsKey(columnName.Key)
                        )
                    )
                    {
                        string str4 = columnName.Value;
                        NameCompatibilityManager._columnName.Add(columnName.Key, str4);
                    }
                }
            }

            NameCompatibilityManager._isInitialized = true;
        }
    }

    /// <summary>
    /// Gets the table name using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns>The string</returns>
    public static string GetTableName(Type type)
    {
        if (!NameCompatibilityManager._isInitialized)
        {
            NameCompatibilityManager.Initialize();
        }

        return NameCompatibilityManager._tableNames.ContainsKey(type)
            ? NameCompatibilityManager._tableNames[type]
            : type.Name;
    }

    /// <summary>
    /// Gets the column name using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The string</returns>
    public static string GetColumnName(Type type, string propertyName)
    {
        if (!NameCompatibilityManager._isInitialized)
        {
            NameCompatibilityManager.Initialize();
        }

        return NameCompatibilityManager._columnName.ContainsKey((type, propertyName))
            ? NameCompatibilityManager._columnName[(type, propertyName)]
            : propertyName;
    }

    /// <summary>
    /// Gets the value of the additional name compatibilities
    /// </summary>
    public static List<Type> AdditionalNameCompatibilities { get; } = new List<Type>();
}
