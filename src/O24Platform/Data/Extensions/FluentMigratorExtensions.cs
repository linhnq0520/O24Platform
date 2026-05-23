using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using LinqToDB.Mapping;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Attributes;
using O24OpenAPI.Data.Mapping;
using O24OpenAPI.Data.Mapping.Builders;

namespace O24OpenAPI.Data.Extensions;

/// <summary>
/// The fluent migrator extensions class
/// </summary>
public static class FluentMigratorExtensions
{
    /// <summary>
    /// The date time precision
    /// </summary>
    private const int DATE_TIME_PRECISION = 6;

    /// <summary>
    /// Gets the value of the type mapping
    /// </summary>
    private static Dictionary<Type, Action<ICreateTableColumnAsTypeSyntax>> TypeMapping { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentMigratorExtensions"/> class
    /// </summary>
    static FluentMigratorExtensions()
    {
        Dictionary<Type, Action<ICreateTableColumnAsTypeSyntax>> dictionary = [];
        Type key1 = typeof(int);
        dictionary[key1] = c => c.AsInt32();
        Type key2 = typeof(long);
        dictionary[key2] = c => c.AsInt64();
        Type key3 = typeof(string);
        dictionary[key3] = c =>
        {
            DataProviderType dataProvider = DataSettingsManager.LoadSettings().DataProvider;

            if (dataProvider == DataProviderType.Oracle)
            {
                c.AsCustom("NCLOB").Nullable();
            }
            else
            {
                c.AsString(int.MaxValue).Nullable();
            }
        };
        Type key4 = typeof(bool);
        dictionary[key4] = c => c.AsBoolean();
        Type key5 = typeof(decimal);
        dictionary[key5] = c => c.AsDecimal(18, 8);
        Type key6 = typeof(DateTime);
        dictionary[key6] = c => c.AsNeptuneDateTime2();
        Type key7 = typeof(byte[]);
        dictionary[key7] = c => c.AsBinary(int.MaxValue);
        Type key8 = typeof(Guid);
        dictionary[key8] = c => c.AsGuid();
        TypeMapping = dictionary;
    }

    /// <summary>
    /// Defines the by own type using the specified column name
    /// </summary>
    /// <param name="columnName">The column name</param>
    /// <param name="propType">The prop type</param>
    /// <param name="create">The create</param>
    /// <param name="canBeNullable">The can be nullable</param>
    /// <exception cref="ArgumentNullException">The column name cannot be empty</exception>
    private static void DefineByOwnType(
        string columnName,
        Type propType,
        CreateTableExpressionBuilder create,
        bool canBeNullable = false
    )
    {
        if (string.IsNullOrEmpty(columnName))
        {
            throw new ArgumentNullException(nameof(columnName), "The column name cannot be empty");
        }

        if (
            propType == typeof(string)
            || propType
                .FindInterfaces(
                    (t, o) =>
                    {
                        string fullName = t.FullName;
                        return fullName != null
                            && fullName.Equals(
                                o.ToString(),
                                StringComparison.InvariantCultureIgnoreCase
                            );
                    },
                    "System.Collections.IEnumerable"
                )
                .Length != 0
        )
        {
            canBeNullable = true;
        }
        ICreateTableColumnAsTypeSyntax columnAsTypeSyntax = create.WithColumn(columnName);
        TypeMapping[propType](columnAsTypeSyntax);
        if (propType == typeof(DateTime))
        {
            create.CurrentColumn.Precision = new int?(6);
        }
        if (!canBeNullable)
        {
            return;
        }
        create.Nullable();
    }

    /// <summary>
    /// Converts the neptune date time 2 using the specified syntax
    /// </summary>
    /// <param name="syntax">The syntax</param>
    /// <returns>The with column syntax</returns>
    public static ICreateTableColumnOptionOrWithColumnSyntax AsNeptuneDateTime2(
        this ICreateTableColumnAsTypeSyntax syntax
    )
    {
        DataProviderType dataProvider = DataSettingsManager.LoadSettings().DataProvider;
        ICreateTableColumnOptionOrWithColumnSyntax withColumnSyntax;
        switch (dataProvider)
        {
            case DataProviderType.SqlServer:
                ICreateTableColumnAsTypeSyntax columnAsTypeSyntax1 = syntax;
                DefaultInterpolatedStringHandler interpolatedStringHandler1 = new(11, 1);
                interpolatedStringHandler1.AppendLiteral("datetime2(");
                interpolatedStringHandler1.AppendFormatted(6);
                interpolatedStringHandler1.AppendLiteral(")");
                string stringAndClear1 = interpolatedStringHandler1.ToStringAndClear();
                withColumnSyntax = columnAsTypeSyntax1.AsCustom(stringAndClear1);
                break;
            case DataProviderType.MySql:
                ICreateTableColumnAsTypeSyntax columnAsTypeSyntax2 = syntax;
                DefaultInterpolatedStringHandler interpolatedStringHandler2 = new(10, 1);
                interpolatedStringHandler2.AppendLiteral("datetime(");
                interpolatedStringHandler2.AppendFormatted(6);
                interpolatedStringHandler2.AppendLiteral(")");
                string stringAndClear2 = interpolatedStringHandler2.ToStringAndClear();
                withColumnSyntax = columnAsTypeSyntax2.AsCustom(stringAndClear2);
                break;
            case DataProviderType.Oracle:
                withColumnSyntax = syntax.AsCustom("TIMESTAMP(6)");

                break;
            default:
                withColumnSyntax = syntax.AsDateTime();
                break;
        }
        if (true)
        {
            ;
        }

        return withColumnSyntax;
    }

    /// <summary>
    /// Foreign the key using the specified column
    /// </summary>
    /// <typeparam name="TPrimary">The primary</typeparam>
    /// <param name="column">The column</param>
    /// <param name="primaryTableName">The primary table name</param>
    /// <param name="primaryColumnName">The primary column name</param>
    /// <param name="onDelete">The on delete</param>
    /// <returns>The create table column option or foreign key cascade or with column syntax</returns>
    public static ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey<TPrimary>(
        this ICreateTableColumnOptionOrWithColumnSyntax column,
        string primaryTableName = null,
        string primaryColumnName = null,
        Rule onDelete = Rule.Cascade
    )
        where TPrimary : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(primaryTableName))
        {
            primaryTableName = NameCompatibilityManager.GetTableName(typeof(TPrimary));
        }
        if (string.IsNullOrWhiteSpace(primaryColumnName))
        {
            primaryColumnName = "Id";
        }
        return column.Indexed().ForeignKey(primaryTableName, primaryColumnName).OnDelete(onDelete);
    }

    /// <summary>
    /// Foreign the key using the specified column
    /// </summary>
    /// <typeparam name="TPrimary">The primary</typeparam>
    /// <param name="column">The column</param>
    /// <param name="primaryTableName">The primary table name</param>
    /// <param name="primaryColumnName">The primary column name</param>
    /// <param name="onDelete">The on delete</param>
    /// <returns>The alter table column option or add column or alter column or foreign key cascade syntax</returns>
    public static IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey<TPrimary>(
        this IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax column,
        string primaryTableName = null,
        string primaryColumnName = null,
        Rule onDelete = Rule.Cascade
    )
        where TPrimary : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(primaryTableName))
        {
            primaryTableName = NameCompatibilityManager.GetTableName(typeof(TPrimary));
        }
        if (string.IsNullOrWhiteSpace(primaryColumnName))
        {
            primaryColumnName = "Id";
        }
        return column.Indexed().ForeignKey(primaryTableName, primaryColumnName).OnDelete(onDelete);
    }

    /// <summary>
    /// Tables the for using the specified expression root
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="expressionRoot">The expression root</param>
    public static void TableFor<TEntity>(this ICreateExpressionRoot expressionRoot)
        where TEntity : BaseEntity
    {
        Type type = typeof(TEntity);
        CreateTableExpressionBuilder builder =
            expressionRoot.Table(NameCompatibilityManager.GetTableName(type))
            as CreateTableExpressionBuilder;

        builder.RetrieveTableExpressions(type);
    }

    /// <summary>
    /// Retrieves the table expressions using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="type">The type</param>
    public static void RetrieveTableExpressions(
        this CreateTableExpressionBuilder builder,
        Type type
    )
    {
        ITypeFinder typeFinder = Singleton<ITypeFinder>.Instance;
        Type type1 = typeFinder
            .FindClassesOfType(typeof(IEntityBuilder))
            .FirstOrDefault(t =>
            {
                bool isValidDbType = true;
                DatabaseTypeAttribute attribute = t.GetCustomAttribute<DatabaseTypeAttribute>();
                if (attribute != null)
                {
                    if (
                        attribute.DatabaseTypes.Contains(
                            DataSettingsManager.LoadSettings().DataProvider
                        )
                    )
                    {
                        isValidDbType = true;
                    }
                    else
                    {
                        isValidDbType = false;
                    }
                }
                Type baseType = t.BaseType;
                return isValidDbType
                    && baseType is not null
                    && baseType.GetGenericArguments().Contains(type);
            });

        if (
            type1 != null
            && EngineContext.Current.ResolveUnregistered(type1) is IEntityBuilder entityBuilder
        )
        {
            entityBuilder.MapEntity(builder);
        }

        CreateTableExpression expression = builder.Expression;

        if (!expression.Columns.Any(c => c.IsPrimaryKey))
        {
            ColumnDefinition columnDefinition = new()
            {
                Name = "Id",
                Type = new DbType?(DbType.Int32),
                IsIdentity = true,
                TableName = NameCompatibilityManager.GetTableName(type),
                ModificationType = ColumnModificationType.Create,
                IsPrimaryKey = true,
            };
            expression.Columns.Insert(0, columnDefinition);
            builder.CurrentColumn = columnDefinition;
        }

        foreach (
            PropertyInfo propertyInfo in type.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public
                )
                .Where(pi =>
                    //pi.DeclaringType != typeof(BaseEntity) &&
                    pi.CanWrite
                    && !pi.HasAttribute<NotMappedAttribute>()
                    && !pi.HasAttribute<NotColumnAttribute>()
                    && !expression.Columns.Any(x =>
                        x.Name.Equals(
                            NameCompatibilityManager.GetColumnName(type, pi.Name),
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    && TypeMapping.ContainsKey(pi.PropertyType.GetTypeToMap().propType)
                )
        )
        {
            string columnName = NameCompatibilityManager.GetColumnName(type, propertyInfo.Name);
            (Type propType, bool canBeNullable) = propertyInfo.PropertyType.GetTypeToMap();
            DefineByOwnType(columnName, propType, builder, canBeNullable);
        }
    }

    /// <summary>
    /// Gets the type to map using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns>The type prop type bool can be nullable</returns>
    public static (Type propType, bool canBeNullable) GetTypeToMap(this Type type)
    {
        Type underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType != null ? (underlyingType, true) : (type, false);
    }

    public static ICreateTableColumnOptionOrWithColumnSyntax AsNCLOB(
        this ICreateTableColumnAsTypeSyntax column
    )
    {
        return column.AsCustom("NCLOB");
    }

    public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsNCLOB(
        this IAlterTableColumnAsTypeSyntax column
    )
    {
        return column.AsCustom("NCLOB");
    }
}
