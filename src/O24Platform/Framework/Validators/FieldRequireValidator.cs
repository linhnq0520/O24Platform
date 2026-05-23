using FluentValidation;
using FluentValidation.Validators;
using O24OpenAPI.Core.Domain.Localization;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Localization;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Validators;

/// <summary>
/// The field require validator class
/// </summary>
/// <seealso cref="PropertyValidator{T, TProperty}"/>
public class FieldRequireValidator<T, TProperty> : PropertyValidator<T, TProperty>
{
    /// <summary>
    /// The field name
    /// </summary>
    private string _fieldName;

    /// <summary>
    /// Gets the value of the name
    /// </summary>
    public override string Name => "FieldRequireValidator";

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldRequireValidator{T,TProperty}"/> class
    /// </summary>
    /// <param name="fieldName">The field name</param>
    public FieldRequireValidator(string fieldName)
    {
        _fieldName = fieldName;
    }

    /// <summary>
    /// Ises the valid using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="value">The value</param>
    /// <returns>The bool</returns>
    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        if (string.IsNullOrEmpty(_fieldName))
        {
            _fieldName = context.PropertyPath;
        }
        TProperty val = value;
        TProperty val2 = val;
        if (val2 != null)
        {
            if (val2 is string || val2 is decimal || val2 is int || val2 is long)
            {
                return !string.IsNullOrWhiteSpace(value.ToString());
            }
            return !EqualityComparer<TProperty>.Default.Equals(value, default(TProperty));
        }
        return false;
    }

    /// <summary>
    /// Gets the default message template using the specified error code
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <returns>The string</returns>
    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        Language result = EngineContext
            .Current.Resolve<IWorkContext>()
            .GetWorkingLanguage()
            .GetAsyncResult();
        string result2 = EngineContext
            .Current.Resolve<ILocalizationService>()
            .GetResource("Common.Value.Required")
            .GetAsyncResult();
        IEntityFieldService entityFieldService =
            EngineContext.Current.Resolve<IEntityFieldService>();
        string arg = _fieldName;
        if (!string.IsNullOrEmpty(_fieldName))
        {
            arg = entityFieldService
                .GetByEntityField(typeof(T).Name, _fieldName, result.UniqueSeoCode)
                .GetAsyncResult();
        }
        return string.Format(result2, arg);
    }
}
