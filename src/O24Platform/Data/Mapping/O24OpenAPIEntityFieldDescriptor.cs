using System.Data;

namespace O24OpenAPI.Data.Mapping;

/// <summary>
/// The 24 open api entity field descriptor class
/// </summary>
public class O24OpenAPIEntityFieldDescriptor
{
    /// <summary>
    /// Gets or sets the value of the name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the is identity
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Gets or sets the value of the is nullable
    /// </summary>
    public bool? IsNullable { get; set; }

    /// <summary>
    /// Gets or sets the value of the is primary key
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the value of the is unique
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Gets or sets the value of the precision
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Gets or sets the value of the size
    /// </summary>
    public int? Size { get; set; }

    /// <summary>
    /// Gets or sets the value of the CLR type
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Gets or sets the database type defined by FluentMigrator
    /// </summary>
    public DbType? DbType { get; set; }
}
