namespace O24OpenAPI.Core;

/// <summary>
/// The 24 open api exception class
/// </summary>
/// <seealso cref="Exception"/>
[Serializable]
public class O24OpenAPIException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIException"/> class
    /// </summary>
    public O24OpenAPIException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    public O24OpenAPIException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIException"/> class
    /// </summary>
    /// <param name="code">The code</param>
    /// <param name="message">The message</param>
    public O24OpenAPIException(string code, string message)
        : base(message)
    {
        this.ErrorCode = code;
    }

    public O24OpenAPIException(string code, string message, string nextAction)
        : base(message)
    {
        this.ErrorCode = code;
        NextAction = nextAction;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIException"/> class
    /// </summary>
    /// <param name="messageFormat">The message format</param>
    /// <param name="args">The args</param>
    public O24OpenAPIException(string messageFormat, params object[] args)
        : base(string.Format(messageFormat, args)) { }

    // /// <summary>
    // /// Initializes a new instance of the <see cref="O24OpenAPIException"/> class
    // /// </summary>
    // /// <param name="info">The info</param>
    // /// <param name="context">The context</param>
    // protected O24OpenAPIException(SerializationInfo info, StreamingContext context)
    //     : base(info, context) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public O24OpenAPIException(string message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIException"/> class
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    /// <param name="sourceLine">The source line</param>
    public O24OpenAPIException(string message, Exception? innerException, string sourceLine)
        : base(message, innerException)
    {
        this.SourceLine = sourceLine;
    }

    /// <summary>
    /// Gets or sets the value of the error code
    /// </summary>
    public virtual string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the source line
    /// </summary>
    public string SourceLine { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;
}
