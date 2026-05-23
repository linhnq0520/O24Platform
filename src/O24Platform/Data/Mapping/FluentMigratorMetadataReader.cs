using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using LinqToDB;
using LinqToDB.Mapping;
using LinqToDB.Metadata;
using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Data.Mapping;

/// <summary>
/// The fluent migrator metadata reader class
/// </summary>
/// <seealso cref="IMetadataReader"/>
public class FluentMigratorMetadataReader(IMappingEntityAccessor mappingEntityAccessor)
    : IMetadataReader
{
    private readonly IMappingEntityAccessor _mappingEntityAccessor = mappingEntityAccessor;

    private static ConcurrentDictionary<(Type, MemberInfo), MappingAttribute> Types { get; } =
        new ConcurrentDictionary<(Type, MemberInfo), MappingAttribute>();

    private T GetAttribute<T>(Type type, MemberInfo memberInfo)
        where T : MappingAttribute
    {
        return (T)
            Types.GetOrAdd(
                (type, memberInfo),
                _ =>
                {
                    O24OpenAPIEntityDescriptor entityDescriptor =
                        _mappingEntityAccessor.GetEntityDescriptor(type);
                    if (typeof(T) == typeof(TableAttribute))
                    {
                        return new TableAttribute(entityDescriptor.EntityName)
                        {
                            Schema = entityDescriptor.SchemaName,
                        };
                    }

                    if (typeof(T) != typeof(ColumnAttribute))
                    {
                        return null;
                    }

                    O24OpenAPIEntityFieldDescriptor entityFieldDescriptor =
                        entityDescriptor.Fields.SingleOrDefault(cd =>
                            cd.Name.Equals(
                                NameCompatibilityManager.GetColumnName(type, memberInfo.Name),
                                StringComparison.OrdinalIgnoreCase
                            )
                        );
                    if (entityFieldDescriptor == null)
                    {
                        return null;
                    }

                    Type type1 = (memberInfo as PropertyInfo)?.PropertyType;
                    type1 ??= typeof(string);
                    Type type2 = type1;
                    MappingSchema mappingSchema = _mappingEntityAccessor.GetMappingSchema();
                    ColumnAttribute attribute = new()
                    {
                        Name = entityFieldDescriptor.Name,
                        IsPrimaryKey = entityFieldDescriptor.IsPrimaryKey,
                        IsColumn = true,
                        CanBeNull = entityFieldDescriptor.IsNullable.GetValueOrDefault(),
                    };

                    int? nullable = entityFieldDescriptor.Size;
                    attribute.Length = nullable.GetValueOrDefault();
                    nullable = entityFieldDescriptor.Precision;
                    attribute.Precision = nullable.GetValueOrDefault();
                    attribute.IsIdentity = entityFieldDescriptor.IsIdentity;

                    bool isEnumStringColumn = false;

                    if (type1.IsEnum && entityFieldDescriptor.DbType is DbType dbType)
                    {
                        switch (dbType)
                        {
                            case DbType.String:
                            case DbType.StringFixedLength:
                                attribute.DataType = DataType.NVarChar;
                                isEnumStringColumn = true;
                                break;
                            case DbType.AnsiString:
                            case DbType.AnsiStringFixedLength:
                                attribute.DataType = DataType.VarChar;
                                isEnumStringColumn = true;
                                break;
                        }

                        if (isEnumStringColumn)
                        {
                            // Map enum values to their string names in the database
                            mappingSchema.SetDefaultFromEnumType(type1, typeof(string));
                        }
                    }

                    if (!isEnumStringColumn)
                    {
                        attribute.DataType = mappingSchema.GetDataType(type2).Type.DataType;
                    }

                    return attribute;
                }
            );
    }

    private MappingAttribute[] GetAttributesInternal<T>(
        Type type,
        Type attributeType,
        MemberInfo memberInfo = null
    )
        where T : MappingAttribute
    {
        T attribute = null;
        int num;
        if (type.IsSubclassOf(typeof(BaseEntity)) && typeof(T) == attributeType)
        {
            attribute = GetAttribute<T>(type, memberInfo);
            num = attribute != null ? 1 : 0;
        }
        else
        {
            num = 0;
        }

        if (num == 0)
        {
            return [];
        }

        return [attribute];
    }

    // Implement IMetadataReader interface methods for LinqToDB 6.1.0
    public MappingAttribute[] GetAttributes(Type type)
    {
        return GetAttributesInternal<TableAttribute>(type, typeof(TableAttribute), null);
    }

    public MappingAttribute[] GetAttributes(Type type, MemberInfo memberInfo)
    {
        return GetAttributesInternal<ColumnAttribute>(type, typeof(ColumnAttribute), memberInfo);
    }

    /// <summary>
    /// Gets the dynamic columns using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns>The member info array</returns>
    public MemberInfo[] GetDynamicColumns(Type type) => [];

    // LinqToDB 6.1.0 requires GetObjectID implementation
    public string GetObjectID()
    {
        // Return a unique identifier for this metadata reader instance
        // This is used for caching purposes in LinqToDB
        return $"{nameof(FluentMigratorMetadataReader)}_{GetHashCode()}";
    }
}
