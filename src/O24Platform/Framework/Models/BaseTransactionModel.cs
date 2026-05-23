using System.Text.Json.Serialization;
using Newtonsoft.Json;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Helpers;

namespace O24OpenAPI.Framework.Models;

/// <summary>Represents base Transaction Model</summary>
public class BaseTransactionModel : BaseO24OpenAPIModel
{
    /// <summary>App code</summary>
    [JsonProperty("app_code")]
    [JsonPropertyName("app_code")]
    public string AppCode { get; set; }

    /// <summary>Transaction code</summary>
    [JsonPropertyName("transaction_code")]
    [JsonProperty("transaction_code")]
    [SwaggerIgnore]
    public string TransactionCode { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("transaction_number")]
    [JsonProperty("transaction_number")]
    [SwaggerIgnore]
    public string TransactionNumber { get; set; }

    /// <summary>
    /// Gets or sets the value of the postings
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public List<GLEntries> Postings { get; set; } = [];

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("transaction_type")]
    [JsonProperty("transaction_type")]
    [SwaggerIgnore]
    public string TransactionType { get; set; }

    /// <summary>Step code</summary>
    [JsonPropertyName("sub_code")]
    [JsonProperty("sub_code")]
    [SwaggerIgnore]
    public string SubCode { get; set; }

    /// <summary>Transaction date</summary>
    [JsonPropertyName("transaction_date")]
    [JsonProperty("transaction_date")]
    [SwaggerIgnore]
    public DateTime TransactionDate { get; set; } = DateTimeOffset.Now.DateTime;

    /// <summary>Service system date</summary>
    [JsonPropertyName("service_sys_date")]
    [JsonProperty("service_sys_date")]
    [SwaggerIgnore]
    public DateTime ServiceSysDate { get; set; } = DateTimeOffset.Now.DateTime;

    /// <summary>Reference identifier</summary>
    [JsonPropertyName("reference_id")]
    [JsonProperty("reference_id")]
    [SwaggerIgnore]
    public string ReferenceId { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("ref_id")]
    [JsonProperty("ref_id")]
    public string RefId { get; set; }

    /// <summary>Reference code</summary>
    [JsonPropertyName("reference_code")]
    [JsonProperty("reference_code")]
    [SwaggerIgnore]
    public string ReferenceCode { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("business_code")]
    [JsonProperty("business_code")]
    [SwaggerIgnore]
    public string BusinessCode { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("value_date")]
    [JsonProperty("value_date")]
    [SwaggerIgnore]
    public DateTime ValueDate { get; set; } = DateTimeOffset.Now.DateTime;

    /// <summary>User code</summary>
    [JsonPropertyName("current_user_code")]
    [JsonProperty("current_user_code")]
    [SwaggerIgnore]
    public string CurrentUserCode { get; set; }

    /// <summary>Branch code</summary>
    [JsonPropertyName("current_branch_code")]
    [JsonProperty("current_branch_code")]
    [SwaggerIgnore]
    public string CurrentBranchCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the current username
    /// </summary>
    [JsonPropertyName("current_username")]
    [JsonProperty("current_username")]
    [SwaggerIgnore]
    public string CurrentUsername { get; set; }

    /// <summary>
    /// Gets or sets the value of the current loginname
    /// </summary>
    [JsonPropertyName("current_loginname")]
    [JsonProperty("current_loginname")]
    [SwaggerIgnore]
    public string CurrentLoginname { get; set; }

    /// <summary>User approve</summary>
    [JsonPropertyName("user_approve")]
    [JsonProperty("user_approve")]
    [SwaggerIgnore]
    public string UserApprove { get; set; }

    /// <summary>Status of transaction</summary>
    [JsonPropertyName("status")]
    [JsonProperty("status")]
    [SwaggerIgnore]
    public string Status { get; set; }

    /// <summary>Is reverse?</summary>
    [JsonPropertyName("is_reverse")]
    [JsonProperty("is_reverse")]
    [SwaggerIgnore]
    public bool IsReverse { get; set; }

    /// <summary>Amount</summary>
    [JsonPropertyName("amount1")]
    [JsonProperty("amount1")]
    [SwaggerIgnore]
    public decimal Amount1 { get; set; }

    /// <summary>Description</summary>
    [JsonPropertyName("description")]
    [JsonProperty("description")]
    [SwaggerIgnore]
    public string Description { get; set; }

    /// <summary>Token</summary>
    [JsonPropertyName("token")]
    [JsonProperty("token")]
    public string Token { get; set; }

    /// <summary>Language</summary>
    [JsonPropertyName("language")]
    [JsonProperty("language")]
    //[Newtonsoft.Json.JsonIgnore]
    //[System.Text.Json.Serialization.JsonIgnore]
    public string Language { get; set; }

    /// <summary>Language</summary>
    [JsonPropertyName("channel_id")]
    [JsonProperty("channel_id")]
    //[Newtonsoft.Json.JsonIgnore]
    //[global::System.Text.Json.Serialization.JsonIgnore]
    public string ChannelId { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("trans_id")]
    [JsonProperty("trans_id")]
    [Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public string TransId { get; set; }

    /// <summary>Request body</summary>
    [JsonPropertyName("request_body")]
    [JsonProperty("request_body")]
    [Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public string RequestBody { get; set; }

    /// <summary>
    /// Gets or sets the value of the ignore session
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public bool IgnoreSession { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the is batch
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public bool IsBatch { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the is system
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the is force reverse
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public bool IsForceReverse { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the is transaction reverse
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [global::System.Text.Json.Serialization.JsonIgnore]
    [JsonPropertyName("is_transaction_reverse")]
    [JsonProperty("is_transaction_reverse")]
    [SwaggerIgnore]
    public bool IsTransactionReverse { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the is transaction compensated
    /// </summary>
    [JsonPropertyName("is_transaction_compensated")]
    [JsonProperty("is_transaction_compensated")]
    [SwaggerIgnore]
    public bool IsTransactionCompensated { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the client device id
    /// </summary>
    [JsonPropertyName("client_device_id")]
    [JsonProperty("client_device_id")]
    public string ClientDeviceId { get; set; }

    [JsonPropertyName("user_base_currency")]
    [JsonProperty("user_base_currency")]
    [SwaggerIgnore]
    public string UserBaseCurrency { get; set; }
}
