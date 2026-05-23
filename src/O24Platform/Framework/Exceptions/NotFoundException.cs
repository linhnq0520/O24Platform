using O24OpenAPI.Core;

namespace O24OpenAPI.Framework.Exceptions;

/// <summary>
/// The not found exception class
/// </summary>
/// <seealso cref="O24OpenAPIException"/>
public class NotFoundException : O24OpenAPIException
{
    /// <summary>
    /// Gets or sets the value of the entity name
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Gets or sets the value of the entity code
    /// </summary>
    public string EntityCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the error code
    /// </summary>
    public override string ErrorCode { get; set; } = "record_not_found";

    /// <summary>
    /// Gets or sets the value of the base model
    /// </summary>
    public BaseO24OpenAPIModel BaseModel { get; set; }

    /// <summary>
    /// Gets the value of the message
    /// </summary>
    public override string Message => $"The {EntityName} with code {EntityCode} was not found";

    /// <summary>
    /// Initializes a new instance of the Exception class
    /// </summary>
    public NotFoundException() { }

    /// <summary>
    /// Initializes a new instance of the Exception class with a specified error message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public NotFoundException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="code">The code</param>
    public NotFoundException(string name, string code)
    {
        EntityName = name;
        EntityCode = code;
        BaseModel = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="code">The code</param>
    /// <param name="defaultModel">The default model</param>
    public NotFoundException(string name, string code, BaseO24OpenAPIModel defaultModel)
    {
        EntityName = name;
        EntityCode = code;
        BaseModel = defaultModel;
    }
}
