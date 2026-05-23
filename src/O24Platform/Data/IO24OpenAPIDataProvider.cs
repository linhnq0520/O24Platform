using System.Linq.Expressions;
using LinqToDB.Data;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data.Mapping;

namespace O24OpenAPI.Data;

/// <summary>
/// The io 24 open api data provider interface
/// </summary>
/// <seealso cref="IMappingEntityAccessor"/>
public interface IO24OpenAPIDataProvider : IMappingEntityAccessor
{
    /// <summary>
    /// Creates the data connection
    /// </summary>
    /// <returns>The data connection</returns>
    DataConnection CreateDataConnection();

    /// <summary>
    /// Creates the database using the specified collation
    /// </summary>
    /// <param name="collation">The collation</param>
    /// <param name="triesToConnect">The tries to connect</param>
    void CreateDatabase(string collation, int triesToConnect = 10);

    /// <summary>
    /// Initializes the database
    /// </summary>
    void InitializeDatabase();

    /// <summary>
    /// Inserts the entity using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    /// <returns>A task containing the entity</returns>
    Task<TEntity> InsertEntity<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity;

    /// <summary>
    /// Updates the entity using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    Task UpdateEntity<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    /// <summary>
    /// Updates the entity fields using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    /// <param name="actions">The actions</param>
    Task UpdateEntityFields<TEntity>(
        TEntity entity,
        List<ActionChain> actions,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity;

    /// <summary>
    /// Updates the entity field using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    Task UpdateEntityField<TEntity>(
        TEntity entity,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity;

    /// <summary>
    /// Updates the entities using the specified entities
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entities">The entities</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    Task UpdateEntities<TEntity>(
        IQueryable<TEntity> entities,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity;

    /// <summary>
    /// Updates the entities using the specified entities
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entities">The entities</param>
    Task UpdateEntities<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity;

    /// <summary>
    /// Deletes the entity using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    Task DeleteEntity<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    /// <summary>
    /// Bulks the delete entities using the specified entities
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entities">The entities</param>
    Task BulkDeleteEntities<TEntity>(
        IList<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity;

    /// <summary>
    /// Bulks the delete entities using the specified predicate
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="predicate">The predicate</param>
    /// <returns>A task containing the int</returns>
    Task<int> BulkDeleteEntities<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        int batchSize = 0,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity;

    /// <summary>
    /// Bulks the insert entities using the specified entities
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entities">The entities</param>
    Task BulkInsertEntities<TEntity>(
        IEnumerable<TEntity> entities,
        bool preserveIdentity = false,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity;

    /// <summary>
    /// Creates the foreign key name using the specified foreign table
    /// </summary>
    /// <param name="foreignTable">The foreign table</param>
    /// <param name="foreignColumn">The foreign column</param>
    /// <param name="primaryTable">The primary table</param>
    /// <param name="primaryColumn">The primary column</param>
    /// <returns>The string</returns>
    string CreateForeignKeyName(
        string foreignTable,
        string foreignColumn,
        string primaryTable,
        string primaryColumn
    );

    /// <summary>
    /// Gets the index name using the specified target table
    /// </summary>
    /// <param name="targetTable">The target table</param>
    /// <param name="targetColumn">The target column</param>
    /// <returns>The string</returns>
    string GetIndexName(string targetTable, string targetColumn);

    /// <summary>
    /// Gets the table
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <returns>A queryable of t entity</returns>
    IQueryable<TEntity> GetTable<TEntity>()
        where TEntity : BaseEntity;

    /// <summary>
    /// Gets the table ident
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <returns>The int</returns>
    int GetTableIdent<TEntity>()
        where TEntity : BaseEntity;

    /// <summary>
    /// Databases the exists
    /// </summary>
    /// <returns>The bool</returns>
    bool DatabaseExists();

    /// <summary>
    /// Backups the database using the specified file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    Task BackupDatabase(string fileName);

    /// <summary>
    /// Restores the database using the specified backup file name
    /// </summary>
    /// <param name="backupFileName">The backup file name</param>
    Task RestoreDatabase(string backupFileName);

    /// <summary>
    /// Res the index tables
    /// </summary>
    Task ReIndexTables();

    /// <summary>
    /// Builds the connection string using the specified neptune connection string
    /// </summary>
    /// <param name="neptuneConnectionString">The neptune connection string</param>
    /// <returns>The string</returns>
    string BuildConnectionString(IO24OpenAPIConnectionStringInfo neptuneConnectionString);

    /// <summary>
    /// Executes the non query using the specified sql
    /// </summary>
    /// <param name="sql">The sql</param>
    /// <param name="dataParameters">The data parameters</param>
    /// <returns>A task containing the int</returns>
    Task<int> ExecuteNonQuery(string sql, params DataParameter[] dataParameters);

    /// <summary>
    /// Queries the proc using the specified procedure name
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="parameters">The parameters</param>
    /// <returns>A task containing a list of t</returns>
    Task<IList<T>> QueryProc<T>(string procedureName, params DataParameter[] parameters);

    /// <summary>
    /// Executes the proc using the specified procedure name
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="parameters">The parameters</param>
    /// <returns>A task containing the int</returns>
    Task<int> ExecuteProc(string procedureName, params DataParameter[] parameters);

    /// <summary>
    /// Queries the sql
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="sql">The sql</param>
    /// <param name="parameters">The parameters</param>
    /// <returns>A task containing a list of t</returns>
    Task<IList<T>> Query<T>(string sql, params DataParameter[] parameters);

    /// <summary>
    /// Truncates the reset identity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="resetIdentity">The reset identity</param>
    Task Truncate<TEntity>(bool resetIdentity = false)
        where TEntity : BaseEntity;

    /// <summary>
    /// Gets the value of the configuration name
    /// </summary>
    string ConfigurationName { get; }

    /// <summary>
    /// Gets the value of the supported length of binary hash
    /// </summary>
    int SupportedLengthOfBinaryHash { get; }

    /// <summary>
    /// Gets the value of the backup supported
    /// </summary>
    bool BackupSupported { get; }

    /// <summary>
    /// Bulks the copy entities using the specified entities
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entities">The entities</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task BulkCopyAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity;
}
