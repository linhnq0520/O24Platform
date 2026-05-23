using System.Reflection;
using FluentValidation;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Framework.Validators;

/// <summary>
/// The validator helper class
/// </summary>
public static class ValidatorHelper
{
    /// <summary>
    /// Gets the validator using the specified obj
    /// </summary>
    /// <param name="obj">The obj</param>
    /// <returns>The validator</returns>
    public static IValidator GetValidator(this BaseO24OpenAPIModel obj)
    {
        try
        {
            Type type1 = typeof(BaseO24OpenAPIValidator<>);
            Type type2 = obj.GetType();
            Type genericType = type1.MakeGenericType(type2);
            return (IValidator)
                EngineContext.Current.ResolveUnregistered(
                    ValidatorHelper.FindValidatorType(type2.Assembly, genericType)
                );
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Finds the validator type using the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly</param>
    /// <param name="genericType">The generic type</param>
    /// <returns>The type</returns>
    private static Type FindValidatorType(Assembly assembly, Type genericType)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(genericType);

        return assembly.GetTypes().FirstOrDefault((Type t) => t.IsSubclassOf(genericType));
    }
}
