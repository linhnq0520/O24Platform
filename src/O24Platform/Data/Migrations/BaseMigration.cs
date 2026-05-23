using System.Linq.Expressions;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Infrastructure;
using LinqToDB;
using LinqToDB.Async;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Utils;

namespace O24OpenAPI.Data.Migrations;

public abstract class BaseMigration : Migration
{
    /// <inheritdoc />
    public abstract override void Up();

    /// <inheritdoc />
    public sealed override void Down() { }

    /// <summary>
    /// Gets the down expressions using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    public override void GetDownExpressions(IMigrationContext context)
    {
        GetUpExpressions(context);
        context.Expressions = context.Expressions.Select(e => e.Reverse()).Reverse().ToList();
    }

    private readonly IO24OpenAPIDataProvider _dataProvider =
        EngineContext.Current.ResolveRequired<IO24OpenAPIDataProvider>();

    public IO24OpenAPIDataProvider DataProvider
    {
        get { return _dataProvider; }
    }

    public IQueryable<TEntity> GetTable<TEntity>()
        where TEntity : BaseEntity
    {
        return _dataProvider.GetTable<TEntity>();
    }

    public async Task SeedData<TEntity>(
        TEntity entity,
        List<string>? conditionKeys = null,
        bool preserveIds = false
    )
        where TEntity : BaseEntity
    {
        List<TEntity> list = new([entity]);
        await SeedData<TEntity>(list, conditionKeys, preserveIds: preserveIds);
    }

    private async Task SeedData<TEntity>(
        List<TEntity> entities,
        List<string>? conditionKeys = null,
        bool isTruncated = false,
        bool preserveIds = false
    )
        where TEntity : BaseEntity
    {
        if (entities.Count == 0)
        {
            return;
        }

        if (isTruncated)
        {
            if (Schema.Table(typeof(TEntity).Name).Exists())
            {
                await _dataProvider.Truncate<TEntity>();
                PrepareEntitiesForInsert(entities, preserveIds);
                await _dataProvider.BulkInsertEntities(entities, preserveIdentity: preserveIds);
            }
        }
        else
        {
            conditionKeys = NormalizeConditionKeys<TEntity>(conditionKeys, preserveIds);

            List<PropertyInfo> keyProperties = typeof(TEntity)
                .GetProperties()
                .Where(prop => conditionKeys.Contains(prop.Name))
                .ToList();

            if (keyProperties.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Entity {typeof(TEntity).Name} does not have properties with the specified key names."
                );
            }
            var toInserts = new List<TEntity>();
            var toUpdates = new List<TEntity>();
            foreach (TEntity item in entities)
            {
                Expression<Func<TEntity, bool>> predicate = BuildPredicate(keyProperties, item);

                List<TEntity> old = await _dataProvider
                    .GetTable<TEntity>()
                    .Where(predicate)
                    .ToListAsync();

                if (old != null && old.Count > 0)
                {
                    try
                    {
                        TEntity oldItem = old.First();
                        if (preserveIds && item.Id != oldItem.Id)
                        {
                            throw new InvalidOperationException(
                                $"Cannot preserve identity for {typeof(TEntity).Name}. Existing Id {oldItem.Id} does not match imported Id {item.Id}."
                            );
                        }

                        DateTime createdOnUtc = oldItem.GetProperty<DateTime>("CreatedOnUtc");
                        if (createdOnUtc != default)
                        {
                            item.CreatedOnUtc = createdOnUtc;
                        }
                        item.UpdatedOnUtc = DateTime.UtcNow;
                        item.Id = oldItem.Id;
                        await _dataProvider.UpdateEntity(item);
                    }
                    catch (Exception ex)
                    {
                        if (preserveIds)
                        {
                            throw;
                        }

                        Console.WriteLine(
                            $"Error updating entity {typeof(TEntity).Name}: {ex.Message}"
                        );
                        await _dataProvider.BulkDeleteEntities(old);
                        PrepareEntityForInsert(item, preserveIds);
                        toInserts.Add(item);
                    }
                }
                else
                {
                    PrepareEntityForInsert(item, preserveIds);
                    toInserts.Add(item);
                }
            }
            if (toInserts.Count > 0)
            {
                await _dataProvider.BulkInsertEntities(toInserts, preserveIdentity: preserveIds);
            }
        }
    }

    /// <summary>
    /// Bulks the delete using the specified predicate
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="predicate">The predicate</param>
    public async Task BulkDelete<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : BaseEntity
    {
        await _dataProvider.BulkDeleteEntities(predicate);
    }

    /// <summary>
    /// Builds the predicate using the specified key properties
    /// </summary>
    private static Expression<Func<TEntity, bool>> BuildPredicate<TEntity>(
        List<PropertyInfo> keyProperties,
        TEntity item
    )
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression predicate = Expression.Constant(true);

        foreach (PropertyInfo keyProperty in keyProperties)
        {
            MemberExpression left = Expression.Property(parameter, keyProperty);
            ConstantExpression right = Expression.Constant(keyProperty.GetValue(item));
            BinaryExpression equality = Expression.Equal(left, right);

            predicate = Expression.AndAlso(predicate, equality);
        }

        return Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
    }

    /// <summary>
    ///
    /// </summary>
    public async Task SeedDataCSV<TEntity>(
        string filePath,
        List<string> conditionKeys,
        bool isTruncate = false
    )
        where TEntity : BaseEntity
    {
        List<TEntity> data = await FileUtils.ReadCSV<TEntity>(filePath);

        await SeedData(data, conditionKeys, isTruncate);
    }

    /// <summary>
    ///
    /// </summary>
    public async Task SeedJsonFolder<TEntity>(
        string folderPath,
        List<string> conditionKeys,
        bool isTruncate = false,
        bool preserveIds = false
    )
        where TEntity : BaseEntity
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException(
                $"Json migration folder was not found: {folderPath}"
            );
        }

        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

        List<TEntity> data = [];
        foreach (string filePath in jsonFiles)
        {
            List<TEntity> dataFile = await FileUtils.ReadJsonStrict<TEntity>(filePath);
            data.AddRange(dataFile);
        }
        await SeedData(data, conditionKeys, isTruncate, preserveIds);
    }

    /// <summary>
    ///
    /// </summary>
    public async Task SeedDataJson<TEntity>(
        string filePath,
        List<string> conditionKeys,
        bool isTruncate = false,
        bool preserveIds = false
    )
        where TEntity : BaseEntity
    {
        List<TEntity> data = await FileUtils.ReadJsonStrict<TEntity>(filePath);
        if (data.Count == 0)
        {
            throw new Exception("No data found in " + filePath);
        }
        await SeedData<TEntity>(data, conditionKeys, isTruncate, preserveIds);
    }

    public Task ImportDataJson<TEntity>(
        string filePath,
        List<string> conditionKeys,
        bool isTruncate = false,
        bool preserveIds = false
    )
        where TEntity : BaseEntity
    {
        return SeedDataJson<TEntity>(filePath, conditionKeys, isTruncate, preserveIds);
    }

    public Task ImportJsonFolder<TEntity>(
        string folderPath,
        List<string> conditionKeys,
        bool isTruncate = false,
        bool preserveIds = false
    )
        where TEntity : BaseEntity
    {
        return SeedJsonFolder<TEntity>(folderPath, conditionKeys, isTruncate, preserveIds);
    }

    public async Task ExportDataJson<TEntity>(
        string filePath,
        string? host = null,
        object? exportedByFields = null
    )
        where TEntity : BaseEntity
    {
        List<TEntity> data = await _dataProvider.GetTable<TEntity>().ToListAsync();
        await FileUtils.WriteJson(filePath, data, host, exportedByFields);
    }

    /// <summary>
    ///
    /// </summary>
    public async Task SeedListData<TEntity>(
        List<TEntity> entities,
        List<string> conditionKeys,
        bool isTruncate = false
    )
        where TEntity : BaseEntity
    {
        await SeedData<TEntity>(entities, conditionKeys, isTruncate);
    }

    /// <summary>
    /// ///
    /// </summary>
    public bool CheckExistData<TEntity>()
        where TEntity : BaseEntity
    {
        return _dataProvider.GetTable<TEntity>().Any();
    }

    private static List<string> NormalizeConditionKeys<TEntity>(
        List<string>? conditionKeys,
        bool preserveIds
    )
        where TEntity : BaseEntity
    {
        if (preserveIds && (conditionKeys == null || conditionKeys.Count == 0))
        {
            return [nameof(BaseEntity.Id)];
        }

        if (conditionKeys == null || conditionKeys.Count == 0)
        {
            throw new ArgumentException(
                $"At least one condition key is required to import {typeof(TEntity).Name} without truncating.",
                nameof(conditionKeys)
            );
        }

        return conditionKeys;
    }

    private static void PrepareEntitiesForInsert<TEntity>(
        IEnumerable<TEntity> entities,
        bool preserveIds
    )
        where TEntity : BaseEntity
    {
        foreach (TEntity entity in entities)
        {
            PrepareEntityForInsert(entity, preserveIds);
        }
    }

    private static void PrepareEntityForInsert<TEntity>(TEntity entity, bool preserveIds)
        where TEntity : BaseEntity
    {
        if (preserveIds && entity.Id <= 0)
        {
            throw new InvalidOperationException(
                $"Cannot preserve identity for {typeof(TEntity).Name} because the imported Id is empty."
            );
        }

        entity.CreatedOnUtc ??= DateTime.UtcNow;
        entity.UpdatedOnUtc ??= DateTime.UtcNow;
    }

    /// <summary>
    /// Executes the script using the specified path to script file
    /// </summary>
    /// <param name="pathToScriptFile">The path to script file</param>
    public void ExecuteScript(string pathToScriptFile)
    {
        base.Execute.Script(pathToScriptFile);
    }
}
