using FluentMigrator.Runner.VersionTableInfo;
using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The migration version info class
/// </summary>
/// <seealso cref="BaseEntity"/>
/// <seealso cref="IVersionTableMetaData"/>
public partial class MigrationVersionInfo : BaseEntity, IVersionTableMetaData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationVersionInfo"/> class
    /// </summary>
    public MigrationVersionInfo()
    {
        this.TableName = nameof(MigrationVersionInfo);
        this.ColumnName = nameof(Version);
        this.DescriptionColumnName = nameof(Description);
        this.AppliedOnColumnName = nameof(AppliedOn);
    }

    /// <summary>
    /// Gets or sets the value of the version
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the value of the applied on
    /// </summary>
    public DateTime AppliedOn { get; set; }

    /// <summary>
    /// Gets or sets the value of the application context
    /// </summary>
    public object ApplicationContext { get; set; }

    /// <summary>
    /// Gets the value of the owns schema
    /// </summary>
    public bool OwnsSchema { get; } = true;

    /// <summary>
    /// Gets the value of the schema name
    /// </summary>
    public string SchemaName { get; } = string.Empty;

    /// <summary>
    /// Gets the value of the table name
    /// </summary>
    public string TableName { get; }

    /// <summary>
    /// Gets the value of the column name
    /// </summary>
    public string ColumnName { get; }

    /// <summary>
    /// Gets the value of the description column name
    /// </summary>
    public string DescriptionColumnName { get; }

    /// <summary>
    /// Gets the value of the unique index name
    /// </summary>
    public string UniqueIndexName { get; } = "UC_Version";

    /// <summary>
    /// Gets the value of the applied on column name
    /// </summary>
    public string AppliedOnColumnName { get; }
}
