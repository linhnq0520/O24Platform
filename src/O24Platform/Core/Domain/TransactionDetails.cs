namespace O24OpenAPI.Core.Domain;

/// <summary>
/// The transaction details class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class TransactionDetails : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the ref id
    /// </summary>
    public string? RefId { get; set; }

    /// <summary>
    /// Gets or sets the value of the reference id
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets the value of the entity
    /// </summary>
    public string? Entity { get; set; }

    /// <summary>
    /// Gets or sets the value of the entity id
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Gets or sets the value of the field name
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Gets or sets the value of the old value
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// Gets or sets the value of the new value
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the value of the update type
    /// </summary>
    public string? UpdateType { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string? Description { get; set; }
}
