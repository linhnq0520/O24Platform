namespace O24OpenAPI.Framework.Models;

public class StoredCommandResponse : BaseO24OpenAPIModel
{
    public StoredCommandResponse() { }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Query { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = int.MaxValue;

    public StoredCommandResponse(
        string id,
        string name,
        string query,
        string type,
        string description,
        DateTime? createdOnUtc,
        DateTime? updatedOnUtc
    )
    {
        Id = id;
        Name = name;
        Query = query;
        Type = type;
        Description = description;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
    }
}
