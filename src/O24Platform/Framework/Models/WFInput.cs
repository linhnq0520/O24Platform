namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The wf input class
/// </summary>
public class WFInput
{
    /// <summary>
    /// Gets or sets the value of the workflowid
    /// </summary>
    public string workflowid { get; set; }

    /// <summary>
    /// Gets or sets the value of the lang
    /// </summary>
    public string lang { get; set; }

    /// <summary>
    /// Gets or sets the value of the token
    /// </summary>
    public string token { get; set; }

    /// <summary>
    /// Gets or sets the value of the reference id
    /// </summary>
    public string reference_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the reference code
    /// </summary>
    public string reference_code { get; set; }

    /// <summary>
    /// Gets or sets the value of the business code
    /// </summary>
    public string business_code { get; set; }

    /// <summary>
    /// Gets or sets the value of the reversal execution id
    /// </summary>
    public string reversal_execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the tran date
    /// </summary>
    public string tran_date { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// Gets or sets the value of the fields
    /// </summary>
    public Dictionary<string, object> fields { get; set; }
}
