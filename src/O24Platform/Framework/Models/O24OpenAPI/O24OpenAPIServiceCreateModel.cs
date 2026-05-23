namespace O24OpenAPI.Framework.Models.O24OpenAPI;

/// <summary>
/// The 24 open api service create model class
/// </summary>
/// <seealso cref="BaseTransactionModel"/>
public class O24OpenAPIServiceModel : BaseTransactionModel
{
    /// <summary>
    /// Gets or sets the value of the step code
    /// </summary>
    public string StepCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the full class name
    /// </summary>
    public string FullClassName { get; set; }

    /// <summary>
    /// Gets or sets the value of the method name
    /// </summary>
    public string MethodName { get; set; }

    /// <summary>
    /// Gets or sets the value of the should await
    /// </summary>
    public bool ShouldAwait { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the is inquiry
    /// </summary>
    public bool IsInquiry { get; set; } = false;
}

public class O24OpenAPIServiceCreateModel : O24OpenAPIServiceModel { }

public class O24OpenAPIServiceUpdateModel : O24OpenAPIServiceModel
{
    /// <summary>
    /// Gets or sets the value of the id
    /// </summary>
    public int Id { get; set; }
}
