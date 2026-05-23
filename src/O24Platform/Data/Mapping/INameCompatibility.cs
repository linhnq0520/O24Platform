namespace O24OpenAPI.Data.Mapping;

/// <summary>
/// The name compatibility interface
/// </summary>
public interface INameCompatibility
{
    /// <summary>
    /// Gets the value of the table names
    /// </summary>
    Dictionary<Type, string> TableNames { get; }

    /// <summary>
    /// Gets the value of the column name
    /// </summary>
    Dictionary<(Type, string), string> ColumnName { get; }
}
