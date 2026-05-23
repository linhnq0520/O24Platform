using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Conventions;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The 24 open api index convention class
/// </summary>
/// <seealso cref="IIndexConvention"/>
public class O24OpenAPIIndexConvention : IIndexConvention
{
    /// <summary>
    /// The data provider
    /// </summary>
    private readonly IO24OpenAPIDataProvider _dataProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIIndexConvention"/> class
    /// </summary>
    /// <param name="dataProvider">The data provider</param>
    public O24OpenAPIIndexConvention(IO24OpenAPIDataProvider dataProvider)
    {
        this._dataProvider = dataProvider;
    }

    /// <summary>
    /// Gets the index name using the specified index
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns>The string</returns>
    private string GetIndexName(IndexDefinition index)
    {
        return this._dataProvider.GetIndexName(
            index.TableName,
            string.Join<string>(
                '_',
                index.Columns.Select<IndexColumnDefinition, string>(
                    (Func<IndexColumnDefinition, string>)(c => c.Name)
                )
            )
        );
    }

    /// <summary>
    /// Applies the expression
    /// </summary>
    /// <param name="expression">The expression</param>
    /// <returns>The expression</returns>
    public IIndexExpression Apply(IIndexExpression expression)
    {
        if (string.IsNullOrEmpty(expression.Index.Name))
        {
            expression.Index.Name = this.GetIndexName(expression.Index);
        }

        return expression;
    }
}
