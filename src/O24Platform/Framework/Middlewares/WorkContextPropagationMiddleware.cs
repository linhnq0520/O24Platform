using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Framework.Extensions;

namespace O24OpenAPI.Framework.Middlewares;

public class WorkContextPropagationMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        WorkContextTemplate workContext = context.GetHeaderValue<WorkContextTemplate>(
            "WorkContext"
        );
        if (workContext is not null)
        {
            context.RequestServices.GetRequiredService<WorkContext>().SetWorkContext(workContext);
        }
        await next(context);
    }
}
