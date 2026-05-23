namespace O24OpenAPI.Core.Domain;

/// <summary>
/// The entity field class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class EntityField : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the service id
    /// </summary>
    public string? ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the value of the entity name
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Gets or sets the value of the field name
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Gets or sets the value of the multi caption
    /// </summary>
    public string? MultiCaption { get; set; }
}
