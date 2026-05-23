namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The transaction detail audit model class
/// </summary>
/// <seealso cref="BaseO24OpenAPIModel"/>
public class TransactionDetailAuditModel : BaseO24OpenAPIModel
{
    /// <summary>
    /// Gets or sets the value of the ref id
    /// </summary>
    public string RefId { get; set; }

    /// <summary>
    /// Gets or sets the value of the entity
    /// </summary>
    public string Entity { get; set; }

    /// <summary>
    /// Gets or sets the value of the entity id
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Gets or sets the value of the field name
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the value of the old value
    /// </summary>
    public string OldValue { get; set; }

    /// <summary>
    /// Gets or sets the value of the new value
    /// </summary>
    public string NewValue { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the value of the update type
    /// </summary>
    public string UpdateType { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string Description { get; set; }
}
