using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The null migration context class
/// </summary>
/// <seealso cref="IMigrationContext"/>
public class NullMigrationContext : IMigrationContext
{
    /// <summary>
    /// Gets or sets the value of the service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// Gets or sets the value of the expressions
    /// </summary>
    public ICollection<IMigrationExpression> Expressions { get; set; } =
        (ICollection<IMigrationExpression>)[];

    /// <summary>
    /// Gets or sets the value of the query schema
    /// </summary>
    public IQuerySchema QuerySchema { get; set; }

#pragma warning disable CS0612 // Type or member is obsolete
    /// <summary>
    /// Gets or sets the value of the migration assemblies
    /// </summary>
    public IAssemblyCollection MigrationAssemblies { get; set; }
#pragma warning restore CS0612 // Type or member is obsolete

    /// <summary>
    /// Gets or sets the value of the application context
    /// </summary>
    public object ApplicationContext { get; set; }

    /// <summary>
    /// Gets or sets the value of the connection
    /// </summary>
    public string Connection { get; set; }
}
