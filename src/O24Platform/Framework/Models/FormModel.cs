using Newtonsoft.Json;
using O24OpenAPI.Client.Events.EventData;
using System.Text.Json;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The form model request class
/// </summary>
/// <seealso cref="BaseTransactionModel"/>
public partial class FormModelRequest : BaseTransactionModel
{
    /// <summary>
    /// Gets or sets the value of the form id
    /// </summary>
    public string FormId { get; set; }

    /// <summary>
    /// Gets or sets the value of the application code
    /// </summary>
    public string ApplicationCode { get; set; }
}

/// <summary>
/// The form request mapping model class
/// </summary>
/// <seealso cref="BaseO24OpenAPIModel"/>
public class FormRequestMappingModel : BaseO24OpenAPIModel
{
    /// <summary>
    /// Gets or sets the value of the table name
    /// </summary>
    [JsonProperty("tablename")]
    public string TableName { get; set; }

    /// <summary>
    /// Gets or sets the value of the action
    /// </summary>
    [JsonProperty("action")]
    public string Action { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    [JsonProperty("data")]
    public Dictionary<string, string> Data { get; set; }
}

/// <summary>
/// The action request model class
/// </summary>
/// <seealso cref="BaseTransactionModel"/>
public class ActionRequestModel : BaseTransactionModel
{
    /// <summary>
    /// Gets or sets the value of the table name
    /// </summary>
    [JsonProperty("tablename")]
    public string TableName { get; set; }

    /// <summary>
    /// Gets or sets the value of the action
    /// </summary>
    [JsonProperty("action")]
    public string Action { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    [JsonProperty("data")]
    public Dictionary<string, object> Data { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionRequestModel"/> class
    /// </summary>
    public ActionRequestModel() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionRequestModel"/> class
    /// </summary>
    /// <param name="entityEvent">The entity event</param>
    public ActionRequestModel(EntityEvent entityEvent)
    {
        TableName = entityEvent.table_name;
        Action = entityEvent.action_type;
        Data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
            System.Text.Json.JsonSerializer.Serialize(entityEvent.data)
        );
    }
}
