using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Data.Utils;

/// <summary>
/// The file utils class
/// </summary>
public class FileUtils
{
    /// <summary>
    /// Reads the csv using the specified path
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="path">The path</param>
    /// <param name="delimiter">The delimiter</param>
    /// <returns>The list data</returns>
    public static async Task<List<TEntity>> ReadCSV<TEntity>(string path, string delimiter = ",")
        where TEntity : BaseEntity
    {
        var listData = new List<TEntity>();

        using (var reader = new StreamReader(path))
        using (
            var csv = new CsvReader(
                reader,
                new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = delimiter,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                }
            )
        )
        {
            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var entity = csv.GetRecord<TEntity>();
                listData.Add(entity);
            }
        }

        return listData;
    }

    /// <summary>
    /// Reads the json using the specified path
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="path">The path</param>
    /// <returns>A task containing a list of t entity</returns>
    public static async Task<List<TEntity>> ReadJson<TEntity>(string path)
        where TEntity : BaseEntity
    {
        try
        {
            return await ReadJsonStrict<TEntity>(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new List<TEntity>();
        }
    }

    /// <summary>
    /// Reads a migration json file and throws when the file is missing or malformed.
    /// Supports the migration envelope [{header}, {info}, {type:"data", data:[...]}]
    /// and a plain array of entities for simple fixtures.
    /// </summary>
    public static async Task<List<TEntity>> ReadJsonStrict<TEntity>(string path)
        where TEntity : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Json file path is required.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Json migration file was not found: {path}", path);
        }

        string content = await File.ReadAllTextAsync(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new JsonException($"Json migration file is empty: {path}");
        }

        JToken root = JToken.Parse(content);
        JToken dataToken = ResolveDataToken(root, path);

        if (dataToken is not JArray dataArray)
        {
            throw new JsonException($"Json migration file data node must be an array: {path}");
        }

        return dataArray.ToObject<List<TEntity>>() ?? [];
    }

    /// <summary>
    /// Writes entities using the same envelope consumed by migration import.
    /// </summary>
    public static async Task WriteJson<TEntity>(
        string path,
        IEnumerable<TEntity> entities,
        string? host = null,
        object? exportedByFields = null
    )
        where TEntity : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Json file path is required.", nameof(path));
        }

        ArgumentNullException.ThrowIfNull(entities);

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            CreateDirectoryIfNotExist(directory);
        }

        JArray root = [];
        root.Add(new JObject { ["type"] = "header", ["command"] = "Export data to Json" });

        JObject info = new()
        {
            ["exported_time"] = DateTime.UtcNow,
            ["db_properties"] = new JArray(
                new JObject { ["name"] = "entity", ["value"] = typeof(TEntity).Name }
            ),
        };

        if (!string.IsNullOrWhiteSpace(host))
        {
            info["host"] = host;
        }

        if (exportedByFields is not null)
        {
            info["exported_by_fields"] = JToken.FromObject(exportedByFields);
        }

        root.Add(info);
        root.Add(
            new JObject
            {
                ["type"] = "data",
                ["data"] = JArray.FromObject(
                    entities,
                    JsonSerializer.Create(
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        }
                    )
                ),
            }
        );

        await File.WriteAllTextAsync(path, root.ToString(Formatting.Indented));
    }

    private static JToken ResolveDataToken(JToken root, string path)
    {
        if (root is JObject rootObject && rootObject["data"] is not null)
        {
            return rootObject["data"];
        }

        if (root is not JArray rootArray)
        {
            throw new JsonException($"Json migration file root must be an array or object: {path}");
        }

        JObject? dataObject = rootArray
            .Children<JObject>()
            .FirstOrDefault(o =>
                string.Equals(o.Value<string>("type"), "data", StringComparison.OrdinalIgnoreCase)
            );

        if (dataObject?["data"] is not null)
        {
            return dataObject["data"];
        }

        if (
            rootArray
                .Children<JObject>()
                .Any(o => o["type"] is not null || o["db_properties"] is not null)
        )
        {
            throw new JsonException($"Json migration file does not contain a data node: {path}");
        }

        return rootArray;
    }

    public static void CreateDirectoryIfNotExist(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static bool FileWriter(string fullPath, string mediaData)
    {
        try
        {
            File.WriteAllText(fullPath, mediaData);
            Console.WriteLine("Media Insert Done at: " + fullPath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex);
            throw;
        }
    }
}
