namespace O24OpenAPI.Data.Mapping;

/// <summary>
/// The 24 open api entity descriptor class
/// </summary>
public class O24OpenAPIEntityDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIEntityDescriptor"/> class
    /// </summary>
    public O24OpenAPIEntityDescriptor()
    {
        this.Fields =
            (ICollection<O24OpenAPIEntityFieldDescriptor>)
                new List<O24OpenAPIEntityFieldDescriptor>();
    }

    /// <summary>
    /// Gets or sets the value of the entity name
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Gets or sets the value of the schema name
    /// </summary>
    public string SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the value of the fields
    /// </summary>
    public ICollection<O24OpenAPIEntityFieldDescriptor> Fields { get; set; }
}
