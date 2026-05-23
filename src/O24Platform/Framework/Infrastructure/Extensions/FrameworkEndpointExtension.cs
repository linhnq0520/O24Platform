using Microsoft.AspNetCore.Builder;

namespace O24OpenAPI.Framework.Infrastructure.Extensions;

public static class FrameworkEndpointExtension
{
    public static void MapFrameworkEndpoint(this WebApplication app)
    {
        app.MapGeneratedEndpoints();
    }
}
