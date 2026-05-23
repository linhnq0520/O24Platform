using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Conventions;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The 24 open api columns convention class
/// </summary>
/// <seealso cref="IColumnsConvention"/>
public class O24OpenAPIColumnsConvention : IColumnsConvention
{
    /// <summary>
    /// Applies the expression
    /// </summary>
    /// <param name="expression">The expression</param>
    /// <returns>The expression</returns>
    public IColumnsExpression Apply(IColumnsExpression expression)
    {
        if (DataSettingsManager.LoadSettings().DataProvider == DataProviderType.PostgreSQL)
        {
            foreach (ColumnDefinition column in expression.Columns)
            {
                DbType? type = column.Type;
                DbType dbType = DbType.String;
                if (type.GetValueOrDefault() == dbType & type.HasValue)
                {
                    column.Type = new DbType?();
                    column.CustomType = "citext";
                }
            }
        }
        return expression;
    }
}
