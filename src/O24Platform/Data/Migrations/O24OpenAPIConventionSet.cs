using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The 24 open api convention set class
/// </summary>
/// <seealso cref="IConventionSet"/>
public class O24OpenAPIConventionSet : IConventionSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIConventionSet"/> class
    /// </summary>
    /// <param name="dataProvider">The data provider</param>
    public O24OpenAPIConventionSet(IO24OpenAPIDataProvider dataProvider)
    {
        ArgumentNullException.ThrowIfNull(dataProvider);
        DefaultConventionSet defaultConventionSet = new DefaultConventionSet();
        this.ForeignKeyConventions =
            (IList<IForeignKeyConvention>)
                new List<IForeignKeyConvention>()
                {
                    (IForeignKeyConvention)new O24OpenAPIForeignKeyConvention(dataProvider),
                    (IForeignKeyConvention)defaultConventionSet.SchemaConvention,
                };
        this.IndexConventions =
            (IList<IIndexConvention>)
                new List<IIndexConvention>()
                {
                    (IIndexConvention)new O24OpenAPIIndexConvention(dataProvider),
                    (IIndexConvention)defaultConventionSet.SchemaConvention,
                };
        this.ColumnsConventions =
            (IList<IColumnsConvention>)
                new List<IColumnsConvention>()
                {
                    (IColumnsConvention)new O24OpenAPIColumnsConvention(),
                    (IColumnsConvention)new DefaultPrimaryKeyNameConvention(),
                };
        this.ConstraintConventions = defaultConventionSet.ConstraintConventions;
        this.SequenceConventions = defaultConventionSet.SequenceConventions;
        this.AutoNameConventions = defaultConventionSet.AutoNameConventions;
        this.SchemaConvention = defaultConventionSet.SchemaConvention;
        this.RootPathConvention = defaultConventionSet.RootPathConvention;
    }

    /// <summary>
    /// Gets the value of the root path convention
    /// </summary>
    public IRootPathConvention RootPathConvention { get; }

    /// <summary>
    /// Gets the value of the schema convention
    /// </summary>
    public DefaultSchemaConvention SchemaConvention { get; }

    /// <summary>
    /// Gets the value of the columns conventions
    /// </summary>
    public IList<IColumnsConvention> ColumnsConventions { get; }

    /// <summary>
    /// Gets the value of the constraint conventions
    /// </summary>
    public IList<IConstraintConvention> ConstraintConventions { get; }

    /// <summary>
    /// Gets the value of the foreign key conventions
    /// </summary>
    public IList<IForeignKeyConvention> ForeignKeyConventions { get; }

    /// <summary>
    /// Gets the value of the index conventions
    /// </summary>
    public IList<IIndexConvention> IndexConventions { get; }

    /// <summary>
    /// Gets the value of the sequence conventions
    /// </summary>
    public IList<ISequenceConvention> SequenceConventions { get; }

    /// <summary>
    /// Gets the value of the auto name conventions
    /// </summary>
    public IList<IAutoNameConvention> AutoNameConventions { get; }
}
