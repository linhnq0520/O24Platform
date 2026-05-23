using System.Collections;
using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Async;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data;
using O24OpenAPI.Data.Configuration;
using O24OpenAPI.Framework.Models.UtilityModels;

namespace O24OpenAPI.Framework.Utils;

public class MigrationJsonImportResult : BaseO24OpenAPIModel
{
    public string EntityName { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// The data utils class
/// </summary>
public class DataUtils
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="requestAddress"></param>
    /// <param name="requestModels"></param>
    /// <param name="entityName"></param>
    /// <returns></returns>
    public static List<FileModel> ExportMultiFiles(
        string requestAddress,
        string entityName,
        List<Dictionary<string, object>> requestModels
    )
    {
        List<FileModel> listFiles = [];
        Type entityType = Singleton<ITypeFinder>.Instance.FindEntityTypeByName(entityName);
        Type typeRepo = typeof(IRepository<>).MakeGenericType(entityType);
        object repo = EngineContext.Current.Resolve(typeRepo);
        System.Reflection.MethodInfo getTableMethod = typeRepo.GetMethod("GetTable");
        IQueryable queryableTable = getTableMethod?.Invoke(repo, null) as IQueryable;

        HashSet<string> requestFields = [];
        ParameterExpression parameter = Expression.Parameter(entityType, "e");
        Expression finalExpression = Expression.Constant(false);

        foreach (Dictionary<string, object> requestModel in requestModels)
        {
            Expression condition = Expression.Constant(true);
            foreach (KeyValuePair<string, object> kvp in requestModel)
            {
                System.Reflection.PropertyInfo prop = entityType.GetProperty(kvp.Key);
                if (prop != null)
                {
                    requestFields.Add(kvp.Key);
                    MemberExpression entityProp = Expression.Property(parameter, prop);
                    ConstantExpression requestValue = Expression.Constant(kvp.Value);
                    BinaryExpression equalExpression = Expression.Equal(entityProp, requestValue);
                    condition = Expression.AndAlso(condition, equalExpression);
                }
            }
            finalExpression = Expression.OrElse(finalExpression, condition);
        }

        LambdaExpression lambda = Expression.Lambda(finalExpression, parameter);
        System.Reflection.MethodInfo whereMethod = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
            .MakeGenericMethod(entityType);
        IQueryable<object> listData =
            (IQueryable<object>)whereMethod.Invoke(null, new object[] { queryableTable, lambda });

        foreach (object item in listData)
        {
            JArray jArray = [];
            string header = $"{{'type':'header','command':'Export data to Json'}}";
            jArray.Add(JToken.Parse(header));

            Dictionary<string, string> dbProperties = GetConnectionInfo();
            JObject info = new() { new JProperty("exported_time", DateTime.UtcNow) };
            if (requestAddress != null)
            {
                info.Add(new JProperty("host", requestAddress));
            }

            info.Add(
                new JProperty(
                    "db_properties",
                    new JArray(
                        new JObject { ["name"] = "server", ["value"] = dbProperties["server"] },
                        new JObject { ["name"] = "port", ["value"] = dbProperties["port"] },
                        new JObject
                        {
                            ["name"] = "database",
                            ["value"] =
                                $"{Singleton<O24OpenAPIConfiguration>.Instance.YourServiceID}",
                        },
                        new JObject { ["name"] = "entity", ["value"] = entityName }
                    )
                )
            );
            info.Add(new JProperty("exported_by_fields", JArray.FromObject(requestModels)));
            jArray.Add(info);

            JArray jListData = [JObject.FromObject(item)];
            JObject data = new()
            {
                new JProperty("type", "data"),
                new JProperty("data", jListData),
            };
            jArray.Add(data);

            string filesName = GenerateFileName(JObject.FromObject(item), requestFields);
            listFiles.Add(
                new FileModel
                {
                    FileContent = jArray.ToString(),
                    FileName = entityName + $"_{filesName}.json",
                    ContentType = "application/json",
                }
            );
        }
        return listFiles;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="requestAddress"></param>
    /// <param name="requestModels"></param>
    /// <param name="entityName"></param>
    /// <returns></returns>
    public static FileModel ExportFile(
        string requestAddress,
        string entityName,
        List<Dictionary<string, object>> requestModels
    )
    {
        List<FileModel> listFiles = [];
        Type entityType = Singleton<ITypeFinder>.Instance.FindEntityTypeByName(entityName);
        Type typeRepo = typeof(IRepository<>).MakeGenericType(entityType);
        object repo = EngineContext.Current.Resolve(typeRepo);
        System.Reflection.MethodInfo getTableMethod = typeRepo.GetMethod("GetTable");
        IQueryable queryableTable = getTableMethod?.Invoke(repo, null) as IQueryable;

        HashSet<string> requestFields = [];
        ParameterExpression parameter = Expression.Parameter(entityType, "e");
        Expression finalExpression = Expression.Constant(false);

        foreach (Dictionary<string, object> requestModel in requestModels)
        {
            Expression condition = Expression.Constant(true);
            foreach (KeyValuePair<string, object> kvp in requestModel)
            {
                System.Reflection.PropertyInfo prop = entityType.GetProperty(kvp.Key);
                if (prop != null)
                {
                    requestFields.Add(kvp.Key);
                    MemberExpression entityProp = Expression.Property(parameter, prop);
                    ConstantExpression requestValue = Expression.Constant(kvp.Value);
                    BinaryExpression equalExpression = Expression.Equal(entityProp, requestValue);
                    condition = Expression.AndAlso(condition, equalExpression);
                }
            }
            finalExpression = Expression.OrElse(finalExpression, condition);
        }

        LambdaExpression lambda = Expression.Lambda(finalExpression, parameter);
        System.Reflection.MethodInfo whereMethod = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
            .MakeGenericMethod(entityType);
        IQueryable<object> listData =
            (IQueryable<object>)whereMethod.Invoke(null, new object[] { queryableTable, lambda });

        JArray jArray = [];
        string header = $@"{{'type':'header','command':'Export data to Json'}}";
        jArray.Add(JToken.Parse(header));

        // info
        Dictionary<string, string> dbProperties = GetConnectionInfo();
        JObject info = new() { new JProperty(name: "exported_time", DateTime.UtcNow) };
        if (requestAddress != null)
        {
            info.Add(new JProperty(name: "host", requestAddress));
        }

        info.Add(
            new JProperty(
                name: "db_properties",
                JArray.Parse(
                    $@"[
                            {{""name"":""server"",""value"":""{dbProperties["server"]}""}},
                            {{""name"":""port"",""value"":""{dbProperties["port"]}""}},
                            {{""name"":""database"",""value"":""{Singleton<O24OpenAPIConfiguration>.Instance.YourServiceID}""}},
                            {{""name"":""entity"",""value"":""{entityName}""}}
                        ]"
                )
            )
        );
        info.Add(
            new JProperty(
                name: "exported_by_fields",
                JArray.Parse(JsonConvert.SerializeObject(requestModels))
            )
        );
        jArray.Add(info.ToObject<JToken>());

        // data
        JArray jListData = JArray.FromObject(listData);
        JObject data = new()
        {
            new JProperty(name: "type", "data"),
            new JProperty(name: "data", jListData),
        };
        jArray.Add(data.ToObject<JToken>());
        return new FileModel
        {
            FileContent = jArray.ToString(),
            FileName = entityName + "Data.json",
            ContentType = "application/json",
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="jObj"></param>
    /// <param name="requestFields"></param>
    /// <returns></returns>
    public static string GenerateFileName(JObject jObj, HashSet<string> requestFields)
    {
        string fileName = "";
        foreach (string item in requestFields)
        {
            if (jObj.ContainsKey(item))
            {
                fileName += "___" + jObj.GetValue(item).ToString();
            }
        }
        return fileName;
    }

    /// <summary>
    /// Gets the connection info
    /// </summary>
    /// <returns>The result</returns>
    public static Dictionary<string, string> GetConnectionInfo()
    {
        Dictionary<string, string> result = [];

        IConfiguration _config = EngineContext.Current.Resolve<IConfiguration>();
        string connString = Singleton<DataConfig>.Instance.ConnectionString;
        string dataProvider = Singleton<DataConfig>.Instance.DataProvider.ToString();
        IEnumerable<string[]> items;
        switch (dataProvider.ToLower())
        {
            case "mysql"
            or "mariadb":
                items = connString
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Split('='));
                foreach (
                    string[] item in items.Where(s =>
                        s[0].ToLower() == "server" || s[0].ToLower() == "port"
                    )
                )
                {
                    result.Add(item[0].ToLower(), item[1]);
                }

                break;
            case "sqlserver":
                string server = connString
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(s => s.Contains("server"));
                string[] temp = server?.Split(',');
                if (temp is not null && temp.Length == 2)
                {
                    result.Add("server", temp[0]);
                    result.Add("port", temp[1]);
                }

                break;
            case "postgresql":
                items = connString
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Split('='));
                foreach (
                    string[] item in items.Where(s =>
                        s[0].ToLower() == "host" || s[0].ToLower() == "port"
                    )
                )
                {
                    result.Add(item[0].ToLower() == "host" ? "server" : item[0].ToLower(), item[1]);
                }

                break;
            case "oracle":
                string dataSource = connString
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(s =>
                        s.Trim().StartsWith("Data Source", StringComparison.OrdinalIgnoreCase)
                    );

                if (dataSource is not null)
                {
                    string parts = dataSource.Split('=')[1];
                    string hostPort = parts.Split('/')[0];
                    string host = hostPort.Split(':')[0];
                    string port = hostPort.Split(':')[1];

                    result.Add("server", host);
                    result.Add("port", port);
                }
                break;
            default:
                break;
        }
        return result;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task<MigrationJsonImportResult> ImportFile(
        string content,
        List<string> filedConstraints,
        bool preserveIds = false
    )
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Json content is required.", nameof(content));
        }

        JArray data = JArray.Parse(content);
        string entityName = GetEntityName(data);

        Type entity = Singleton<ITypeFinder>.Instance.FindEntityTypeByName(entityName);
        if (entity == null)
        {
            throw new TypeLoadException($"Entity type '{entityName}' was not found.");
        }

        List<System.Reflection.PropertyInfo> keyProperties = ValidateImportConstraints(
            entity,
            filedConstraints,
            preserveIds
        );

        Type typeRepo = typeof(IRepository<>).MakeGenericType(entity);
        object repo = EngineContext.Current.Resolve(typeRepo);
        if (repo == null)
        {
            throw new InvalidOperationException($"Could not resolve IRepository<{entity.Name}>.");
        }

        System.Reflection.MethodInfo getTableMethod = typeRepo.GetMethod("GetTable");
        System.Reflection.MethodInfo insertMethod = typeRepo.GetMethod("Insert");
        System.Reflection.MethodInfo updateMethod = typeRepo.GetMethod("Update");
        IO24OpenAPIDataProvider dataProvider =
            EngineContext.Current.Resolve<IO24OpenAPIDataProvider>()
            ?? throw new InvalidOperationException("Could not resolve IO24OpenAPIDataProvider.");

        object queryableTable = getTableMethod?.Invoke(repo, null);

        JToken listData = GetDataToken(data);
        MigrationJsonImportResult result = new() { EntityName = entityName };

        foreach (JToken item in listData)
        {
            object entityObj = item.ToObject(entity);
            if (entityObj == null)
            {
                result.Skipped++;
                result.Errors.Add($"Could not deserialize item for entity {entityName}.");
                continue;
            }

            IList listDbEntities = await GetExistingEntities(
                entity,
                queryableTable,
                keyProperties,
                entityObj
            );
            if (listDbEntities.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Import key for {entityName} is not unique. Keys: {string.Join(", ", filedConstraints)}"
                );
            }

            if (listDbEntities.Count == 1)
            {
                PreserveDatabaseIdentity(
                    (BaseEntity)entityObj,
                    (BaseEntity)listDbEntities[0],
                    preserveIds,
                    entityName
                );
                await InvokeRepositoryTask(updateMethod, repo, entityObj);
                result.Updated++;
            }
            else
            {
                if (preserveIds)
                {
                    await InvokeBulkInsertPreserveIdentity(dataProvider, entity, entityObj);
                }
                else
                {
                    await InvokeRepositoryTask(insertMethod, repo, entityObj);
                }
                result.Inserted++;
            }
        }

        return result;
    }

    public static async Task<MigrationJsonImportResult> ImportFolder(
        string folderPath,
        List<string> filedConstraints,
        bool preserveIds = false
    )
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException(
                $"Json migration folder was not found: {folderPath}"
            );
        }

        MigrationJsonImportResult summary = new() { EntityName = Path.GetFileName(folderPath) };
        foreach (string filePath in Directory.GetFiles(folderPath, "*.json"))
        {
            string content = await File.ReadAllTextAsync(filePath);
            MigrationJsonImportResult fileResult = await ImportFile(
                content,
                filedConstraints,
                preserveIds
            );

            summary.EntityName = fileResult.EntityName;
            summary.Inserted += fileResult.Inserted;
            summary.Updated += fileResult.Updated;
            summary.Skipped += fileResult.Skipped;
            summary.Errors.AddRange(fileResult.Errors);
        }

        return summary;
    }

    private static string GetEntityName(JArray data)
    {
        JObject info =
            data.Children<JObject>().FirstOrDefault(o => o["db_properties"] is not null)
            ?? throw new JsonException("Json migration file does not contain db_properties.");

        JArray dbProperties =
            info["db_properties"] as JArray
            ?? throw new JsonException("Json migration file db_properties must be an array.");

        string entityName = dbProperties
            .Children<JObject>()
            .FirstOrDefault(o =>
                string.Equals(o.Value<string>("name"), "entity", StringComparison.OrdinalIgnoreCase)
            )
            ?.Value<string>("value");

        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new JsonException("Json migration file does not define db_properties entity.");
        }

        return entityName;
    }

    private static JToken GetDataToken(JArray data)
    {
        JToken listData = data.Children<JObject>()
            .FirstOrDefault(o =>
                string.Equals(o.Value<string>("type"), "data", StringComparison.OrdinalIgnoreCase)
            )
            ?["data"];

        if (listData is not JArray)
        {
            throw new JsonException("Json migration file data node must be an array.");
        }

        return listData;
    }

    private static List<System.Reflection.PropertyInfo> ValidateImportConstraints(
        Type entity,
        List<string> filedConstraints,
        bool preserveIds
    )
    {
        if (preserveIds && (filedConstraints == null || filedConstraints.Count == 0))
        {
            filedConstraints = [nameof(BaseEntity.Id)];
        }

        if (filedConstraints == null || filedConstraints.Count == 0)
        {
            throw new ArgumentException("At least one import constraint field is required.");
        }

        List<System.Reflection.PropertyInfo> keyProperties = [];
        foreach (
            string field in filedConstraints.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct()
        )
        {
            System.Reflection.PropertyInfo property = entity.GetProperty(field.Trim());
            if (property == null)
            {
                throw new ArgumentException(
                    $"Entity {entity.Name} does not contain key field '{field}'."
                );
            }

            keyProperties.Add(property);
        }

        return keyProperties;
    }

    private static async Task<IList> GetExistingEntities(
        Type entity,
        object queryableTable,
        List<System.Reflection.PropertyInfo> keyProperties,
        object entityObj
    )
    {
        ParameterExpression parameter = Expression.Parameter(entity, "x");
        Expression predicate = Expression.Constant(true);

        foreach (System.Reflection.PropertyInfo property in keyProperties)
        {
            object value = property.GetValue(entityObj);
            MemberExpression propertyAccess = Expression.Property(parameter, property);
            ConstantExpression valueExpression = Expression.Constant(value, property.PropertyType);
            BinaryExpression equality = Expression.Equal(propertyAccess, valueExpression);

            predicate = Expression.AndAlso(predicate, equality);
        }

        LambdaExpression lambda = Expression.Lambda(predicate, parameter);

        System.Reflection.MethodInfo whereMethod = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
            .MakeGenericMethod(entity);

        object filteredData = whereMethod.Invoke(null, new object[] { queryableTable, lambda });

        System.Reflection.MethodInfo toListAsyncMethod = typeof(AsyncExtensions)
            .GetMethod(nameof(AsyncExtensions.ToListAsync))
            ?.MakeGenericMethod(entity);

        Task task = (Task)
            toListAsyncMethod.Invoke(null, new object[] { filteredData, CancellationToken.None });
        await task.ConfigureAwait(false);

        return (IList)task.GetType().GetProperty("Result")?.GetValue(task);
    }

    private static async Task InvokeRepositoryTask(
        System.Reflection.MethodInfo method,
        object repo,
        object entityObj
    )
    {
        Task task = (Task)method.Invoke(repo, new[] { entityObj, CancellationToken.None });
        await task.ConfigureAwait(false);
    }

    private static async Task InvokeBulkInsertPreserveIdentity(
        IO24OpenAPIDataProvider dataProvider,
        Type entity,
        object entityObj
    )
    {
        BaseEntity importedEntity = (BaseEntity)entityObj;
        if (importedEntity.Id <= 0)
        {
            throw new InvalidOperationException(
                $"Cannot preserve identity for {entity.Name} because the imported Id is empty."
            );
        }

        Type listType = typeof(List<>).MakeGenericType(entity);
        IList list = (IList)Activator.CreateInstance(listType);
        list.Add(entityObj);

        System.Reflection.MethodInfo bulkInsertMethod = typeof(IO24OpenAPIDataProvider)
            .GetMethod(nameof(IO24OpenAPIDataProvider.BulkInsertEntities))
            ?.MakeGenericMethod(entity);

        Task task = (Task)
            bulkInsertMethod.Invoke(
                dataProvider,
                new object[] { list, CancellationToken.None, true }
            );
        await task.ConfigureAwait(false);
    }

    private static void PreserveDatabaseIdentity(
        BaseEntity importedEntity,
        BaseEntity dbEntity,
        bool preserveIds,
        string entityName
    )
    {
        if (preserveIds && importedEntity.Id != dbEntity.Id)
        {
            throw new InvalidOperationException(
                $"Cannot preserve identity for {entityName}. Existing Id {dbEntity.Id} does not match imported Id {importedEntity.Id}."
            );
        }

        importedEntity.Id = dbEntity.Id;
        importedEntity.CreatedOnUtc = dbEntity.CreatedOnUtc;
        importedEntity.UpdatedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Exports the all using the specified request address
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="requestAddress">The request address</param>
    /// <returns>A task containing the file model</returns>
    public static async Task<FileModel> ExportAll<TEntity>(string requestAddress)
        where TEntity : BaseEntity
    {
        IRepository<TEntity> repo = EngineContext.Current.Resolve<IRepository<TEntity>>();

        List<TEntity> listData = await repo.Table.ToListAsync();
        if (listData.Any())
        {
            // header
            JArray jArray = [];
            string header = $@"{{'type':'header','command':'Export data to Json'}}";
            jArray.Add(JToken.Parse(header));

            // info
            Dictionary<string, string> dbProperties = GetConnectionInfo();
            JObject info = new() { new JProperty(name: "exported_time", DateTime.UtcNow) };
            if (requestAddress != null)
            {
                info.Add(new JProperty(name: "host", requestAddress));
            }

            info.Add(
                new JProperty(
                    name: "db_properties",
                    JArray.Parse(
                        $@"[
                            {{""name"":""server"",""value"":""{dbProperties["server"]}""}},
                            {{""name"":""port"",""value"":""{dbProperties["port"]}""}},
                            {{""name"":""database"",""value"":""cms""}},
                            {{""name"":""entity"",""value"":""{typeof(TEntity).Name}""}}
                        ]"
                    )
                )
            );

            info.Add(new JProperty(name: "exported_by_fields", "Initial data"));
            jArray.Add(info.ToObject<JToken>());

            // data
            JArray jListData = JArray.FromObject(listData);
            JObject data = new()
            {
                new JProperty(name: "type", "data"),
                new JProperty(name: "data", jListData),
            };
            jArray.Add(data.ToObject<JToken>());

            return new FileModel
            {
                FileContent = jArray.ToString(),
                FileName = typeof(TEntity).Name + "Data.json",
                ContentType = "application/json",
            };
        }

        return new FileModel();
    }
}
