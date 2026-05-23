using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Framework.Localization;

namespace O24OpenAPI.Framework.Exceptions;

public class O24Exception : Exception
{
    public string ErrorCode { get; set; } = string.Empty;
    public object[] Values { get; set; } = [];

    public O24Exception() { }

    public O24Exception(string errorCode, params object[] values)
        : base(errorCode)
    {
        ErrorCode = errorCode;
        Values = values;
    }

    public Task<O24OpenAPIException> ToO24OpenAPIExceptionAsync()
    {
        return CreateAsync(ErrorCode, Values);
    }

    public static async Task<O24OpenAPIException> CreateAsync(
        string resourceCode,
        string lang = "en",
        params object[] values
    )
    {
        var workContext = EngineContext.Current.Resolve<WorkContext>();
        lang = lang.Coalesce(workContext.WorkingLanguage, "en");
        var _localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var error = await _localizationService.GetByName(resourceCode, lang);

        if (error == null)
        {
            return new O24OpenAPIException(resourceCode, resourceCode);
        }

        var message =
            values.Length > 0 ? string.Format(error.ResourceValue, values) : error.ResourceValue;
        try
        {
            await message.WriteErrorAsync(resourceCode);
        }
        catch { }
        return new O24OpenAPIException(error.ResourceCode, message);
    }

    public static async Task<O24OpenAPIException> CreateAsync(
        string resourceCode,
        params object[] values
    )
    {
        var workContext = EngineContext.Current.Resolve<WorkContext>();
        var lang = workContext?.WorkingLanguage.Coalesce("en") ?? "en";
        var _localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var error = await _localizationService.GetByName(resourceCode, lang);

        if (error == null)
        {
            return new O24OpenAPIException(resourceCode, resourceCode);
        }

        var message =
            values.Length > 0 ? string.Format(error.ResourceValue, values) : error.ResourceValue;

        return new O24OpenAPIException(resourceCode, message);
    }

    public static async Task<ExceptionWithNextAction> CreateWithNextActionAsync(
        string resourceName,
        string nextAction,
        string lang = "en",
        params object[] values
    )
    {
        lang = lang.Coalesce("en");
        var _localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var error = await _localizationService.GetByName(resourceName, lang);

        if (error == null)
        {
            return new ExceptionWithNextAction(resourceName, resourceName, nextAction);
        }
        var message =
            values.Length > 0 ? string.Format(error.ResourceValue, values) : error.ResourceValue;
        try
        {
            await message.WriteErrorAsync(resourceName);
        }
        catch { }
        return new ExceptionWithNextAction(error.ResourceCode, message, nextAction);
    }
}
