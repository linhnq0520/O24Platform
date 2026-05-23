namespace O24OpenAPI.Framework.Models.JwtModels;

public class ValidateTokenResponseModel
{
    public bool IsValid { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string UserId { get; set; }
}
