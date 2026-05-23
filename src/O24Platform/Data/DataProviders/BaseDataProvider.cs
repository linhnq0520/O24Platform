using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.Linq;
using LinqToDB.Mapping;
using LinqToDB.Tools;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Mapping;
using O24OpenAPI.Data.Migrations;

namespace O24OpenAPI.Data.DataProviders;

public abstract class BaseDataProvider : IMappingEntityAccessor
{
    protected static ConcurrentDictionary<
        Type,
        O24OpenAPIEntityDescriptor
    > EntityDescriptors { get; } = new ConcurrentDictionary<Type, O24OpenAPIEntityDescriptor>();
    protected abstract IDataProvider LinqToDbDataProvider { get; }
    public string ConfigurationName => LinqToDbDataProvider.Name;

    /// <summary>
    /// Gets the internal db connection using the specified connection string
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <returns>The db connection</returns>
    protected abstract DbConnection GetInternalDbConnection(string connectionString);

    /// <summary>
    /// Creates the data connection
    /// </summary>
    /// <returns>The data connection</returns>
    public virtual DataConnection CreateDataConnection()
    {
        return CreateDataConnection(LinqToDbDataProvider);
    }

    /// <summary>
    /// Creates the db command using the specified sql
    /// </summary>
    /// <param name="sql">The sql</param>
    /// <param name="dataParameters">The data parameters</param>
    /// <returns>The command info</returns>
    protected virtual CommandInfo CreateDbCommand(string sql, DataParameter[] dataParameters)
    {
        ArgumentNullException.ThrowIfNull(dataParameters);

        using DataConnection dataConnection = CreateDataConnection(LinqToDbDataProvider);
        return new CommandInfo(dataConnection, sql, dataParameters);
    }

    /// <summary>
    /// Creates the data connection using the specified data provider
    /// </summary>
    /// <param name="dataProvider">The data provider</param>
    /// <returns>The data connection</returns>
    protected virtual DataConnection CreateDataConnection(IDataProvider dataProvider)
    {
        ArgumentNullException.ThrowIfNull(dataProvider);

        var options = new DataOptions()
            .UseConnection(dataProvider, CreateDbConnection())
            .UseMappingSchema(GetMappingSchema());
        return new DataConnection(options)
        {
            CommandTimeout = DataSettingsManager.GetSqlCommandTimeout(),
        };
    }

    /// <summary>
    /// Creates the db connection using the specified connection string
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <returns>The db connection</returns>
    protected virtual DbConnection CreateDbConnection(string? connectionString = null)
    {
        return GetInternalDbConnection(
            (!string.IsNullOrEmpty(connectionString))
                ? connectionString
                : GetCurrentConnectionString()
        );
    }

    /// <summary>
    /// Initializes the database
    /// </summary>
    public virtual void InitializeDatabase()
    {
        IMigrationManager? migrationManager = EngineContext.Current.Resolve<IMigrationManager>();
        migrationManager?.ApplyUpMigrations(typeof(O24OpenAPIDbStartup).Assembly);
    }

    /// <summary>
    /// Gets the entity descriptor using the specified entity type
    /// </summary>
    /// <param name="entityType">The entity type</param>
    /// <returns>The 24 open api entity descriptor</returns>
    public virtual O24OpenAPIEntityDescriptor GetEntityDescriptor(Type entityType)
    {
        return EntityDescriptors.GetOrAdd(
            entityType,
            delegate(Type t)
            {
                string tableName = NameCompatibilityManager.GetTableName(t);
                string schemaName = AppSettingsHelper.GetSchemaName(t);
                CreateTableExpression expression = new() { TableName = tableName };
                CreateTableExpressionBuilder createTableExpressionBuilder = new(
                    expression,
                    new NullMigrationContext()
                );
                createTableExpressionBuilder.RetrieveTableExpressions(t);
                return ConfigurationName.Equals("Oracle.Managed")
                    ? new O24OpenAPIEntityDescriptor
                    {
                        EntityName = tableName.ToUpper(),
                        SchemaName = createTableExpressionBuilder.Expression.SchemaName,
                        Fields =
                        [
                            .. createTableExpressionBuilder.Expression.Columns.Select(
                                column => new O24OpenAPIEntityFieldDescriptor
                                {
                                    Name = column.Name.ToUpper(),
                                    IsPrimaryKey = column.IsPrimaryKey,
                                    IsNullable = column.IsNullable,
                                    Size = column.Size,
                                    Precision = column.Precision,
                                    IsIdentity = column.IsIdentity,
                                    Type = GetPropertyTypeByColumnName(t, column.Name),
                                    DbType = column.Type,
                                }
                            ),
                        ],
                    }
                    : new O24OpenAPIEntityDescriptor
                    {
                        EntityName = tableName,
                        SchemaName = createTableExpressionBuilder.Expression.SchemaName,
                        Fields =
                        [
                            .. createTableExpressionBuilder.Expression.Columns.Select(
                                column => new O24OpenAPIEntityFieldDescriptor
                                {
                                    Name = column.Name,
                                    IsPrimaryKey = column.IsPrimaryKey,
                                    IsNullable = column.IsNullable,
                                    Size = column.Size,
                                    Precision = column.Precision,
                                    IsIdentity = column.IsIdentity,
                                    Type = GetPropertyTypeByColumnName(t, column.Name),
                                    DbType = column.Type,
                                }
                            ),
                        ],
                    };
            }
        );

        static Type GetPropertyTypeByColumnName(Type targetType, string name)
        {
            try
            {
                return Array
                    .Find(
                        targetType.GetProperties(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty
                        ),
                        pi =>
                            name.Equals(NameCompatibilityManager.GetColumnName(targetType, pi.Name))
                    )
                    .PropertyType.GetTypeToMap()
                    .propType;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[ERROR] Failed to get property type for column '{name}' in type '{targetType.Name}': {ex.Message}"
                );
                throw;
            }
        }
    }

    /// <summary>
    /// Gets the mapping schema
    /// </summary>
    /// <returns>The mapping schema</returns>
    public MappingSchema GetMappingSchema()
    {
        return Singleton<MappingSchema>.Instance
            ?? (Singleton<MappingSchema>.Instance = CreateMappingSchema());
    }

    private MappingSchema CreateMappingSchema()
    {
        MappingSchema mappingSchema = new(ConfigurationName, LinqToDbDataProvider.MappingSchema);

        // LinqToDB 6.1.0: Use AddMetadataReader instead of property assignment
        FluentMigratorMetadataReader metadataReader = new(this);
        mappingSchema.AddMetadataReader(metadataReader);

        return mappingSchema;
    }

    /// <summary>
    /// Gets the table
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <returns>A queryable of t entity</returns>
    public virtual IQueryable<TEntity> GetTable<TEntity>()
        where TEntity : BaseEntity
    {
        string schemaName = AppSettingsHelper.GetSchemaName(typeof(TEntity));
        using var connection = CreateDataConnection(LinqToDbDataProvider);

        // LinqToDB 6.1.0: Use DataOptions instead of direct constructor parameters
        DataOptions dataOptions = new DataOptions()
            .UseConnectionString(LinqToDbDataProvider, GetCurrentConnectionString())
            .UseMappingSchema(GetMappingSchema())
            .UseCommandTimeout(DataSettingsManager.GetSqlCommandTimeout());

        DataContext dataContext = new(dataOptions);

        if (!string.IsNullOrEmpty(schemaName))
        {
            return dataContext.GetTable<TEntity>().SchemaName(schemaName); // DatabaseName() đổi thành SchemaName()
        }

        return dataContext.GetTable<TEntity>();
    }

    /// <summary>
    /// Inserts the entity using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    /// <returns>The entity</returns>
    public virtual async Task<TEntity> InsertEntity<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        await using DataConnection dataContext = CreateDataConnection();
        entity.Id = await dataContext.InsertWithInt32IdentityAsync(
            entity,
            token: cancellationToken
        );
        return entity;
    }

    public virtual async Task UpdateEntity<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        using DataConnection dataContext = CreateDataConnection();
        await dataContext.UpdateAsync(entity, token: cancellationToken);
    }

    public IUpdatable<TEntity> SetEntityField<TEntity>(
        TEntity entity,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, bool>> predicate = e => e.Id == entity.Id;
        Expression<Func<TEntity, string>> extract = x => Sql.Property<string>(x, propertyName);
        return GetTable<TEntity>().Where(predicate).Set(extract, value);
    }

    private static IUpdatable<TEntity> SetEntityField<TEntity>(
        IUpdatable<TEntity> updateChains,
        string propertyName,
        int value,
        string actionType = "U",
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, int>> extract = x => Sql.Property<int>(x, propertyName);
        Expression<Func<TEntity, int>> expression = x => Sql.Property<int>(x, propertyName);
        expression =
            (actionType == "C")
                ? (x => Sql.Property<int>(x, propertyName) + value)
                : (
                    (!(actionType == "D"))
                        ? (x => value)
                        : (x => Sql.Property<int>(x, propertyName) - value)
                );
        return updateChains.Set(extract, expression);
    }

    /// <summary>
    /// Sets the entity field using the specified update chains
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="updateChains">The update chains</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    /// <param name="actionType">The action type</param>
    /// <returns>An updatable of t entity</returns>
    private static IUpdatable<TEntity> SetEntityField<TEntity>(
        IUpdatable<TEntity> updateChains,
        string propertyName,
        long value,
        string actionType = "U",
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, long>> extract = x => Sql.Property<long>(x, propertyName);
        Expression<Func<TEntity, long>> expression = x => Sql.Property<long>(x, propertyName);
        expression =
            (actionType == "C")
                ? (x => Sql.Property<long>(x, propertyName) + value)
                : (
                    (!(actionType == "D"))
                        ? (x => value)
                        : (x => Sql.Property<long>(x, propertyName) - value)
                );
        return updateChains.Set(extract, expression);
    }

    /// <summary>
    /// Sets the entity field using the specified update chains
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="updateChains">The update chains</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    /// <param name="actionType">The action type</param>
    /// <returns>An updatable of t entity</returns>
    private static IUpdatable<TEntity> SetEntityField<TEntity>(
        IUpdatable<TEntity> updateChains,
        string propertyName,
        decimal value,
        string actionType = "U",
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, decimal>> extract = x => Sql.Property<decimal>(x, propertyName);
        Expression<Func<TEntity, decimal>> expression = x => Sql.Property<decimal>(x, propertyName);
        expression =
            (actionType == "C")
                ? (x => Sql.Property<decimal>(x, propertyName) + value)
                : (
                    (!(actionType == "D"))
                        ? (x => value)
                        : (x => Sql.Property<decimal>(x, propertyName) - value)
                );
        return updateChains.Set(extract, expression);
    }

    /// <summary>
    /// Sets the entity null field using the specified update chains
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="updateChains">The update chains</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    /// <param name="actionType">The action type</param>
    /// <returns>An updatable of t entity</returns>
    private static IUpdatable<TEntity> SetEntityNullField<TEntity>(
        IUpdatable<TEntity> updateChains,
        string propertyName,
        decimal value,
        string actionType = "U",
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, decimal?>> extract = x => Sql.Property<decimal?>(x, propertyName);
        Expression<Func<TEntity, decimal?>> expression = x =>
            Sql.Property<decimal?>(x, propertyName);
        expression =
            (actionType == "C")
                ? (
                    x =>
                        ((Sql.Property<decimal?>(x, propertyName) * 100000m) + (value * 100000m))
                        / 100000m
                )
                : (
                    (!(actionType == "D"))
                        ? (x => value)
                        : (
                            x =>
                                (
                                    (Sql.Property<decimal?>(x, propertyName) * 100000m)
                                    - (value * 100000m)
                                ) / 100000m
                        )
                );
        return updateChains.Set(extract, expression);
    }

    /// <summary>
    /// Sets the entity null field using the specified update chains
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="updateChains">The update chains</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    /// <param name="actionType">The action type</param>
    /// <returns>An updatable of t entity</returns>
    private static IUpdatable<TEntity> SetEntityNullField<TEntity>(
        IUpdatable<TEntity> updateChains,
        string propertyName,
        int value,
        string actionType = "U",
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, int?>> extract = x => Sql.Property<int?>(x, propertyName);
        Expression<Func<TEntity, int?>> expression = x => Sql.Property<int?>(x, propertyName);
        expression =
            (actionType == "C")
                ? (x => Sql.Property<int?>(x, propertyName) + value)
                : (
                    (!(actionType == "D"))
                        ? (x => value)
                        : (x => Sql.Property<int?>(x, propertyName) - value)
                );
        return updateChains.Set(extract, expression);
    }

    /// <summary>
    /// Sets the entity enum field using the specified update chains
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="updateChains">The update chains</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    /// <param name="actionType">The action type</param>
    /// <returns>An updatable of t entity</returns>
    private static IUpdatable<TEntity> SetEntityEnumField<TEntity>(
        IUpdatable<TEntity> updateChains,
        string propertyName,
        object value,
        string actionType = "U",
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, int>> extract = x => Sql.Property<int>(x, propertyName);
        Expression<Func<TEntity, int>> expression = x => Sql.Property<int>(x, propertyName);
        expression = x => 1;
        return updateChains.Set(extract, expression);
    }

    /// <summary>
    /// Sets the entity field using the specified update chains
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="updateChains">The update chains</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    /// <param name="actionType">The action type</param>
    /// <returns>An updatable of t entity</returns>
    private static IUpdatable<TEntity> SetEntityField<TEntity>(
        IUpdatable<TEntity> updateChains,
        string propertyName,
        string value,
        string actionType = "U",
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, string>> extract = x => Sql.Property<string>(x, propertyName);
        return updateChains.Set(extract, value);
    }

    /// <summary>
    /// Sets the entity field using the specified update chains
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="updateChains">The update chains</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    /// <param name="actionType">The action type</param>
    /// <returns>An updatable of t entity</returns>
    private static IUpdatable<TEntity> SetEntityField<TEntity>(
        IUpdatable<TEntity> updateChains,
        string propertyName,
        DateTime value,
        string actionType = "U",
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, DateTime>> extract = x => Sql.Property<DateTime>(x, propertyName);
        return updateChains.Set(extract, value);
    }

    /// <summary>
    /// Sets the entity field using the specified update chains
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="updateChains">The update chains</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    /// <param name="actionType">The action type</param>
    /// <returns>An updatable of t entity</returns>
    private static IUpdatable<TEntity> SetEntityField<TEntity>(
        IUpdatable<TEntity> updateChains,
        string propertyName,
        object value,
        string actionType = "U",
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, object>> extract = x => Sql.Property<object>(x, propertyName);
        return updateChains.Set(extract, value);
    }

    /// <summary>
    /// Updates the entity fields using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    /// <param name="actions">The actions</param>
    public virtual async Task UpdateEntityFields<TEntity>(
        TEntity entity,
        List<ActionChain> actions,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        if (entity == null || actions == null || !actions.Any())
        {
            return;
        }

        IQueryable<TEntity> query = Queryable.Where(
            predicate: e => e.Id == entity.Id,
            source: GetTable<TEntity>()
        );
        Expression<Func<TEntity, DateTime?>> updateExp = x =>
            Sql.Property<DateTime?>(x, "UpdatedOnUtc");
        IUpdatable<TEntity> updateChains = query.Set(updateExp, DateTime.UtcNow);
        PropertyInfo propInfo = entity
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(x => x.Name.Equals("UpdatedOnUtc", StringComparison.OrdinalIgnoreCase));
        if ((object)propInfo == null)
        {
            Expression<Func<TEntity, int>> updateId = x => Sql.Property<int>(x, "Id");
            updateChains = query.Set(updateId, entity.Id);
        }

        foreach (ActionChain action in actions)
        {
            Type type = entity.GetType();
            List<string> fields = action.UpdateFields;
            if (fields == null || fields.Count == 0)
            {
                fields = [action.UpdateField];
            }

            string actionType = action.Action;
            foreach (string field in fields)
            {
                PropertyInfo prop = type.GetProperty(field);
                if (prop.PropertyType == typeof(decimal?))
                {
                    updateChains = SetEntityNullField(
                        updateChains,
                        field,
                        Convert.ToDecimal(action.UpdateValue),
                        actionType,
                        cancellationToken
                    );
                    continue;
                }

                if (prop.PropertyType == typeof(int?))
                {
                    updateChains = SetEntityNullField(
                        updateChains,
                        field,
                        Convert.ToInt32(action.UpdateValue),
                        actionType,
                        cancellationToken
                    );
                    continue;
                }

                switch (Type.GetTypeCode(prop.PropertyType))
                {
                    case TypeCode.Int16:
                        updateChains = SetEntityField(
                            updateChains,
                            field,
                            Convert.ToInt16(action.UpdateValue),
                            actionType,
                            cancellationToken
                        );
                        break;
                    case TypeCode.Int32:
                        if (!prop.PropertyType.IsEnum)
                        {
                            updateChains = SetEntityField(
                                updateChains,
                                field,
                                Convert.ToInt32(action.UpdateValue),
                                actionType,
                                cancellationToken
                            );
                        }

                        break;
                    case TypeCode.Int64:
                        updateChains = SetEntityField(
                            updateChains,
                            field,
                            Convert.ToInt64(action.UpdateValue),
                            actionType,
                            cancellationToken
                        );
                        break;
                    case TypeCode.Decimal:
                        updateChains = SetEntityField(
                            updateChains,
                            field,
                            Convert.ToDecimal(action.UpdateValue),
                            actionType,
                            cancellationToken
                        );
                        break;
                    case TypeCode.String:
                        updateChains = SetEntityField(
                            updateChains,
                            field,
                            action.UpdateValue.ToString(),
                            "U",
                            cancellationToken
                        );
                        break;
                    case TypeCode.DateTime:
                        updateChains = SetEntityField(
                            updateChains,
                            field,
                            Convert.ToDateTime(action.UpdateValue),
                            cancellationToken: cancellationToken
                        );
                        break;
                    case TypeCode.Object:
                        updateChains = SetEntityField(
                            updateChains,
                            field,
                            action.UpdateValue,
                            cancellationToken: cancellationToken
                        );
                        break;
                }
            }
        }

        await updateChains.UpdateAsync(token: cancellationToken);
    }

    /// <summary>
    /// Updates the entity field using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    public virtual async Task UpdateEntityField<TEntity>(
        TEntity entity,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        await SetEntityField(entity, propertyName, value, cancellationToken)
            .UpdateAsync(token: cancellationToken);
    }

    /// <summary>
    /// Updates the entities using the specified entities
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entities">The entities</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    public virtual async Task UpdateEntities<TEntity>(
        IQueryable<TEntity> entities,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        string entityName = entities.GetType().GenericTypeArguments[0].Name;
        PropertyInfo prop = ReflectionHelper.FindProperty(entityName, propertyName);
        if (prop.PropertyType == typeof(bool))
        {
            await entities
                .Set(x => Sql.Property<bool>(x, propertyName), bool.Parse(value))
                .UpdateAsync(token: cancellationToken);
        }
        else
        {
            await entities
                .Set(x => Sql.Property<string>(x, propertyName), value)
                .UpdateAsync(token: cancellationToken);
        }
    }

    /// <summary>
    /// Updates the entities using the specified entities
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entities">The entities</param>
    public virtual async Task UpdateEntities<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        foreach (TEntity entity in entities)
        {
            await UpdateEntity(entity, cancellationToken);
        }
    }

    /// <summary>
    /// Deletes the entity using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    public virtual async Task DeleteEntity<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        using DataConnection dataContext = CreateDataConnection();
        await dataContext.DeleteAsync(entity, token: cancellationToken);
    }

    /// <summary>
    /// Bulks the delete entities using the specified entities
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entities">The entities</param>
    public virtual async Task BulkDeleteEntities<TEntity>(
        IList<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        using DataConnection dataContext = CreateDataConnection();
        if (entities.All(entity => entity.Id == 0))
        {
            foreach (TEntity entity in entities)
            {
                await dataContext.DeleteAsync(entity, token: cancellationToken);
            }

            return;
        }

        await (
            from e in dataContext.GetTable<TEntity>()
            where e.Id.In(entities.Select(x => x.Id))
            select e
        ).DeleteAsync(token: cancellationToken);
    }

    /// <summary>
    /// Bulks the delete entities using the specified predicate
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="predicate">The predicate</param>
    /// <returns>A task containing the int</returns>
    public virtual async Task<int> BulkDeleteEntities<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        int batchSize = 0,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        using DataConnection dataContext = CreateDataConnection();
        if (batchSize > 0)
        {
            return await dataContext
                .GetTable<TEntity>()
                .Where(predicate)
                .Take(batchSize)
                .DeleteAsync(token: cancellationToken);
        }
        return await dataContext
            .GetTable<TEntity>()
            .Where(predicate)
            .DeleteAsync(token: cancellationToken);
    }

    /// <summary>
    /// Bulks the insert entities using the specified entities
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entities">The entities</param>
    public virtual async Task BulkInsertEntities<TEntity>(
        IEnumerable<TEntity> entities,
        bool preserveIdentity = false,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        using DataConnection dataContext = CreateDataConnection(LinqToDbDataProvider);
        List<TEntity> entityList = entities.ToList();
        if (entityList.Count == 0)
        {
            return;
        }

        if (preserveIdentity)
        {
            throw new NotSupportedException(
                $"Preserve identity bulk insert is not supported by {GetType().Name}."
            );
        }

        await dataContext.BulkCopyAsync(
            new BulkCopyOptions(),
            entityList.RetrieveIdentity(dataContext),
            cancellationToken: cancellationToken
        );
    }

    public Task BulkCopyAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        using DataConnection dataContext = CreateDataConnection(LinqToDbDataProvider);

        return dataContext.BulkCopyAsync(
            new BulkCopyOptions(),
            entities.RetrieveIdentity(dataContext),
            cancellationToken
        );
    }

    /// <summary>
    /// Gets the default identity impl using the specified context
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="context">The context</param>
    /// <param name="sourceList">The source list</param>
    /// <param name="entityDescriptor">The entity descriptor</param>
    /// <param name="column">The column</param>
    /// <param name="sqlBuilder">The sql builder</param>
    private static void GetDefaultIdentityImplLinq<T>(
        DataConnection context,
        IList<T> sourceList,
        EntityDescriptor entityDescriptor,
        ColumnDescriptor column
    )
        where T : notnull
    {
        // Get max value using raw SQL with proper escaping
        string tableName = entityDescriptor.Name.Name;
        string schemaName = entityDescriptor.Name.Schema;
        string columnName = column.ColumnName;

        // Build full table name
        string fullTableName = string.IsNullOrEmpty(schemaName)
            ? $"[{tableName}]"
            : $"[{schemaName}].[{tableName}]";

        // Execute query
        string maxValueSql = $"SELECT ISNULL(MAX([{columnName}]), 0) FROM {fullTableName}";

        int maxValue = context.Execute<int>(maxValueSql);

        // Assign incremented values
        foreach (T source in sourceList)
        {
            maxValue++;
            Console.WriteLine($"max value: {maxValue}");
            column.MemberAccessor.SetValue(source, maxValue);
        }
    }

    /// <summary>
    /// Executes the non query using the specified sql
    /// </summary>
    /// <param name="sql">The sql</param>
    /// <param name="dataParameters">The data parameters</param>
    /// <returns>A task containing the int</returns>
    public virtual async Task<int> ExecuteNonQuery(
        string sql,
        params DataParameter[] dataParameters
    )
    {
        CommandInfo command = CreateDbCommand(sql, dataParameters);
        return await command.ExecuteAsync();
    }

    /// <summary>
    /// Queries the proc using the specified procedure name
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="parameters">The parameters</param>
    /// <returns>A task containing a list of t</returns>
    public virtual Task<IList<T>> QueryProc<T>(
        string procedureName,
        params DataParameter[] parameters
    )
    {
        CommandInfo commandInfo = CreateDbCommand(procedureName, parameters);
        List<T> list = commandInfo.QueryProc<T>()?.ToList();
        return Task.FromResult((IList<T>)(list ?? []));
    }

    /// <summary>
    /// Executes the proc using the specified procedure name
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="parameters">The parameters</param>
    /// <returns>A task containing the int</returns>
    public virtual async Task<int> ExecuteProc(
        string procedureName,
        params DataParameter[] parameters
    )
    {
        using DataConnection dataContext = CreateDataConnection();
        return await dataContext.ExecuteProcAsync(procedureName, parameters);
    }

    /// <summary>
    /// Queries the sql
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="sql">The sql</param>
    /// <param name="parameters">The parameters</param>
    /// <returns>A task containing a list of t</returns>
    public virtual Task<IList<T>> Query<T>(string sql, params DataParameter[] parameters)
    {
        using DataConnection connection = CreateDataConnection();
        return Task.FromResult((IList<T>)(connection.Query<T>(sql, parameters)?.ToList() ?? []));
    }

    /// <summary>
    /// Truncates the reset identity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="resetIdentity">The reset identity</param>
    public virtual async Task Truncate<TEntity>(bool resetIdentity = false)
        where TEntity : BaseEntity
    {
        using DataConnection dataContext = CreateDataConnection();
        await dataContext.GetTable<TEntity>().TruncateAsync(resetIdentity);
    }

    /// <summary>
    /// Gets the current connection string
    /// </summary>
    /// <returns>The string</returns>
    protected static string GetCurrentConnectionString()
    {
        return DataSettingsManager.LoadSettings().ConnectionString;
    }

    /// <summary>
    /// Gets the service connection string using the specified db name
    /// </summary>
    /// <param name="dbName">The db name</param>
    /// <returns>The string</returns>
    protected static string GetServiceConnectionString(string dbName)
    {
        return DataSettingsManager.LoadSettings().ConnectionString;
    }
}
