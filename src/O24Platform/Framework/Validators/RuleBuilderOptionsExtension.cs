using FluentValidation;
using FluentValidation.Validators;
using O24OpenAPI.Core.Extensions;

namespace O24OpenAPI.Framework.Validators;

/// <summary>
/// The rule builder options extension class
/// </summary>
public static class RuleBuilderOptionsExtension
{
    /// <summary>
    /// Adds the mesage await using the specified rule
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TProperty">The property</typeparam>
    /// <param name="rule">The rule</param>
    /// <param name="errorMessage">The error message</param>
    /// <returns>A rule builder options of t and t property</returns>
    public static IRuleBuilderOptions<T, TProperty> WithMesageAwait<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Task<string> errorMessage
    )
    {
        return DefaultValidatorOptions.WithMessage(rule, errorMessage.GetAsyncResult());
    }

    /// <summary>
    /// Adds the mesage await using the specified rule
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TProperty">The property</typeparam>
    /// <param name="rule">The rule</param>
    /// <param name="errorMessage">The error message</param>
    /// <returns>A rule builder options of t and t property</returns>
    public static IRuleBuilderOptions<T, TProperty> WithMesageAwait<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Func<Task<string>> errorMessage
    )
    {
        return DefaultValidatorOptions.WithMessage(rule, errorMessage().GetAsyncResult());
    }

    /// <summary>
    /// Adds the message await using the specified rule
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TProperty">The property</typeparam>
    /// <param name="rule">The rule</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="args">The args</param>
    /// <returns>A rule builder options of t and t property</returns>
    public static IRuleBuilderOptions<T, TProperty> WithMessageAwait<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Task<string> errorMessage,
        params object[] args
    )
    {
        return DefaultValidatorOptions.WithMessage(
            rule,
            string.Format(errorMessage.GetAsyncResult(), args)
        );
    }

    /// <summary>
    /// Musts the await using the specified rule builder
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TProperty">The property</typeparam>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <param name="predicate">The predicate</param>
    /// <returns>A rule builder options of t and t property</returns>
    public static IRuleBuilderOptions<T, TProperty> MustAwait<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<T, TProperty, Task<bool>> predicate
    )
    {
        return DefaultValidatorExtensions.Must(
            ruleBuilder,
            (T x, TProperty context) => predicate(x, context).GetAsyncResult()
        );
    }

    /// <summary>
    /// Whens the await using the specified rule
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TProperty">The property</typeparam>
    /// <param name="rule">The rule</param>
    /// <param name="predicate">The predicate</param>
    /// <param name="applyConditionTo">The apply condition to</param>
    /// <returns>A rule builder options of t and t property</returns>
    public static IRuleBuilderOptions<T, TProperty> WhenAwait<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Func<T, Task<bool>> predicate,
        ApplyConditionTo applyConditionTo = 0
    )
    {
        //IL_001b: Unknown result type (might be due to invalid IL or missing references)
        return DefaultValidatorOptions.When(
            rule,
            (T x) => predicate(x).GetAsyncResult(),
            applyConditionTo
        );
    }

    /// <summary>
    /// Musts the require using the specified rule builder
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TProperty">The property</typeparam>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <param name="fieldName">The field name</param>
    /// <returns>A rule builder options of t and t property</returns>
    public static IRuleBuilderOptions<T, TProperty> MustRequire<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        string fieldName
    )
    {
        return ruleBuilder.SetValidator(
            (IPropertyValidator<T, TProperty>)
                (object)new FieldRequireValidator<T, TProperty>(fieldName)
        );
    }

    /// <summary>
    /// Musts the require using the specified rule builder
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TProperty">The property</typeparam>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <returns>A rule builder options of t and t property</returns>
    public static IRuleBuilderOptions<T, TProperty> MustRequire<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(
            (IPropertyValidator<T, TProperty>)(object)new FieldRequireValidator<T, TProperty>("")
        );
    }
}
