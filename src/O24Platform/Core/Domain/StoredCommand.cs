namespace O24OpenAPI.Core.Domain;

/// <summary>
/// The stored command class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class StoredCommand : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the name
    /// /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the query
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Gets or sets the value of the type
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string? Description { get; set; }
}
