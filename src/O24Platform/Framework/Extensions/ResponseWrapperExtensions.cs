using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using O24OpenAPI.Framework.Attributes;

namespace O24OpenAPI.Framework.Extensions;

public static class ResponseWrapperExtensions
{
    public static bool ShouldSkipResponseWrapper(this HttpContext context) =>
        context.GetEndpoint()?.Metadata.GetMetadata<SkipResponseWrapperAttribute>() is not null;

    public static RouteHandlerBuilder WithoutResponseWrapper(this RouteHandlerBuilder builder) =>
        builder.WithMetadata(new SkipResponseWrapperAttribute());
}
