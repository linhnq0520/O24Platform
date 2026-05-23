using FluentValidation.AspNetCore;

namespace O24OpenAPI.Framework.Validators;

/// <summary>
/// The validate attribute class
/// </summary>
/// <seealso cref="CustomizeValidatorAttribute"/>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class ValidateAttribute : CustomizeValidatorAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateAttribute"/> class
    /// </summary>
    public ValidateAttribute()
    {
        RuleSet = O24OpenAPIValidationDefaults.ValidationRuleSet;
    }
}
