using Newtonsoft.Json.Linq;
using O24OpenAPI.Core.Extensions;

namespace O24OpenAPI.Framework.Extensions;

public static class StringExtensions
{
    private static readonly string defaultLanguage = "en";

    public static string GetLangValue(this string input, string lang = null)
    {
        if (string.IsNullOrEmpty(lang))
        {
            lang = defaultLanguage;
        }

        if (input.TryParse(out JObject ob))
        {
            var langValue = ob[lang];
            if (langValue.IsEmptyOrNull())
            {
                langValue = ob[lang];
                if (langValue.IsEmptyOrNull())
                {
                    return string.Empty;
                }
            }
            return langValue.ToString();
        }

        return string.Empty;
    }
}
