using System.Text.Json.Serialization;
using O24OpenAPI.Core;

namespace O24OpenAPI.Contracts.Models;

public class BaseResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("execution_id")]
    public string? ExecutionId { get; set; }

    [JsonPropertyName("time_in_milliseconds")]
    public long TimeInMilliseconds { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("stack_trace")]
    public string? StackTrace { get; set; }

    [JsonPropertyName("error_next_action")]
    public string? ErrorNextAction { get; set; }

    [JsonPropertyName("transaction_number")]
    public object? TransactionNumber { get; set; }

    [JsonPropertyName("transaction_date")]
    public object? TransactionDate { get; set; }

    [JsonPropertyName("value_date")]
    public object? ValueDate { get; set; }

    [JsonPropertyName("approval_request_id")]
    public string? ApprovalRequestId { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public BaseResponse() { }

    public BaseResponse(T? data)
    {
        Data = data;
        Success = true;
    }

    public BaseResponse(Exception ex)
    {
        Success = false;
        ErrorMessage = ex.Message;
    }

    public BaseResponse(O24OpenAPIException ex)
    {
        Success = false;
        ErrorCode = ex.ErrorCode;
        ErrorMessage = ex.Message;
    }
}
