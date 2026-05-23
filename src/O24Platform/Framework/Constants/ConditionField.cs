using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Constants;

/// <summary>
/// The condition field class
/// </summary>
public class ConditionField
{
    /// <summary>
    /// The step code
    /// </summary>
    public static readonly List<string> O24OpenAPIServiceCondition =
    [
        nameof(O24OpenAPIService.StepCode),
    ];
}
