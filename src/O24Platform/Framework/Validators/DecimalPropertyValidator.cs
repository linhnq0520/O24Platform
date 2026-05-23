using FluentValidation;
using FluentValidation.Validators;

namespace O24OpenAPI.Framework.Validators;

/// <summary>
/// The decimal property validator class
/// </summary>
/// <seealso cref="PropertyValidator{T, TProperty}"/>
public class DecimalPropertyValidator<T, TProperty> : PropertyValidator<T, TProperty>
{
    /// <summary>
    /// The max value
    /// </summary>
    private readonly decimal _maxValue;

    /// <summary>
    /// Gets the value of the name
    /// </summary>
    public override string Name => "DecimalPropertyValidator";

    /// <summary>
    /// Initializes a new instance of the <see cref="DecimalPropertyValidator{T,TProperty}"/> class
    /// </summary>
    /// <param name="maxValue">The max value</param>
    public DecimalPropertyValidator(decimal maxValue)
    {
        _maxValue = maxValue;
    }

    /// <summary>
    /// Gets the default message template using the specified error code
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <returns>The string</returns>
    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return "Decimal value is out of range";
    }

    /// <summary>
    /// Ises the valid using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="value">The value</param>
    /// <returns>The bool</returns>
    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        if (decimal.TryParse(value.ToString(), out var result))
        {
            return Math.Round(result, 8) < _maxValue;
        }
        return false;
    }
}
