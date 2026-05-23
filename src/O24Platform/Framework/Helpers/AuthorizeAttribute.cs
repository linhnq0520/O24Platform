using Microsoft.AspNetCore.Mvc.Filters;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Framework.Helpers;

/// <summary>
///
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter, IFilterMetadata
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        EngineContext.Current.Resolve<WebApiSettings>();
    }
}
