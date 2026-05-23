using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Conventions;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The 24 open api foreign key convention class
/// </summary>
/// <seealso cref="IForeignKeyConvention"/>
public class O24OpenAPIForeignKeyConvention : IForeignKeyConvention
{
    /// <summary>
    /// The data provider
    /// </summary>
    private readonly IO24OpenAPIDataProvider _dataProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIForeignKeyConvention"/> class
    /// </summary>
    /// <param name="dataProvider">The data provider</param>
    public O24OpenAPIForeignKeyConvention(IO24OpenAPIDataProvider dataProvider)
    {
        this._dataProvider = dataProvider;
    }

    /// <summary>
    /// Gets the foreign key name using the specified foreign key
    /// </summary>
    /// <param name="foreignKey">The foreign key</param>
    /// <returns>The string</returns>
    private string GetForeignKeyName(ForeignKeyDefinition foreignKey)
    {
        string foreignColumn = string.Join<string>(
            '_',
            (IEnumerable<string>)foreignKey.ForeignColumns
        );
        string primaryColumn = string.Join<string>(
            '_',
            (IEnumerable<string>)foreignKey.PrimaryColumns
        );
        return this._dataProvider.CreateForeignKeyName(
            foreignKey.ForeignTable,
            foreignColumn,
            foreignKey.PrimaryTable,
            primaryColumn
        );
    }

    /// <summary>
    /// Applies the expression
    /// </summary>
    /// <param name="expression">The expression</param>
    /// <returns>The expression</returns>
    public IForeignKeyExpression Apply(IForeignKeyExpression expression)
    {
        if (string.IsNullOrEmpty(expression.ForeignKey.Name))
        {
            expression.ForeignKey.Name = this.GetForeignKeyName(expression.ForeignKey);
        }

        return expression;
    }
}
