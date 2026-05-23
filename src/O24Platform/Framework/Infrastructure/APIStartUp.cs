using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Utils;

namespace O24OpenAPI.Framework.Infrastructure;

/// <summary>
/// The api start up class
/// </summary>
/// <seealso cref="IO24OpenAPIStartup"/>
public class APIStartUp : IO24OpenAPIStartup
{
    /// <summary>
    /// Configures the services using the specified services
    /// </summary>
    /// <param name="services">The services</param>
    /// <param name="configuration">The configuration</param>
    /// <exception cref="ArgumentException">O24OPENAPI_ENVIRONMENT is not set in environment variables.</exception>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<IMemoryCacheService, MemoryCacheManager>();
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                opts.JsonSerializerOptions.Converters.Add(
                    new System.Text.Json.Serialization.JsonStringEnumConverter()
                );
            });
        services.Configure<RouteOptions>(o =>
        {
            o.LowercaseUrls = true;
        });

        services.AddGrpc(options =>
        {
            options.MaxReceiveMessageSize = new int?();
            options.MaxSendMessageSize = new int?();
        });
        var env =
            Environment.GetEnvironmentVariable("O24OPENAPI_ENVIRONMENT")
            ?? throw new ArgumentException(
                "O24OPENAPI_ENVIRONMENT is not set in environment variables."
            );
        if (
            !Singleton<O24OpenAPIConfiguration>.Instance.Environment.Equals(
                "Dev",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            services.Configure<MvcOptions>(options =>
            {
                var partManager = services
                    .BuildServiceProvider()
                    .GetRequiredService<ApplicationPartManager>();

                var frameworkXPart = partManager.ApplicationParts.FirstOrDefault(p =>
                    p.Name == "O24OpenAPI.Framework"
                );

                if (frameworkXPart != null)
                {
                    partManager.ApplicationParts.Remove(frameworkXPart);
                }
            });
        }

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo()
                {
                    Title =
                        "JITS O24OpenAPI "
                        + Singleton<AppSettings>
                            .Instance.Get<O24OpenAPIConfiguration>()
                            .YourServiceID
                        + " API",
                    Version = "10.0",
                    Description =
                        "JITS O24OpenApi "
                        + Singleton<AppSettings>
                            .Instance.Get<O24OpenAPIConfiguration>()
                            .YourServiceID
                        + " API. Current version 10.0",
                }
            );

            //options.SchemaFilter<SwaggerExcludeSchemaFilter>();

            SwaggerFilterUtils.ApplyAllFilters(options);

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme()
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = WebApiCommonDefaults.SecurityHeaderName,
                    Description =
                        @"**Bearer Token Authorization**:
                    Enter your token in the format `Bearer {token}` to authenticate requests. This API uses two types of Bearer tokens with distinct purposes:
                    - **Static Token**:
                      - **Purpose**: Used for the `/api/auth/loginopenapi` endpoint to initiate authentication.
                      - **How to obtain**: Pre-configured token provided by the system administrator (contact support@jits.com.vn).
                      - **Example**: `Bearer static-token-123456789`
                      - **Note**: This token have been provided by JITS Company.
                    - **Dynamic Token (JWT)**:
                      - **Purpose**: Used for all subsequent endpoints after successful login, requiring user-specific permissions.
                      - **How to obtain**: Obtained from the `/api/auth/loginopenapi` endpoint with valid credentials (LoginName and Password).
                      - **Example**: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
                      - **Note**: Token expires based on the configured lifetime; refresh using the refresh token if provided.",
                }
            );
            var fullUris = GetOpenApiUris();
            foreach (var uri in fullUris)
            {
                options.AddServer(
                    new OpenApiServer() { Url = uri, Description = $"Server at {uri}" }
                );
            }
        });

        // =========================
        // 🔥 CORS
        // =========================

        var allowedOriginsSetting = configuration["O24OpenAPIConfiguration:AllowedCorsOrigins"];

        services.AddCors(options =>
        {
            if (env.Equals("dev", StringComparison.OrdinalIgnoreCase))
            {
                options.AddPolicy(
                    "CorsPolicy",
                    policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    }
                );
            }
            else
            {
                var origins = (allowedOriginsSetting ?? string.Empty).Split(
                    ';',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );

                if (origins.Length == 0)
                {
                    Console.WriteLine(
                        "⚠️ WARNING: AllowedCorsOrigins not set → USING AllowAnyOrigin()"
                    );

                    options.AddPolicy(
                        "CorsPolicy",
                        policy =>
                        {
                            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                        }
                    );
                }
                else
                {
                    options.AddPolicy(
                        "CorsPolicy",
                        policy =>
                        {
                            policy.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader();
                        }
                    );
                }
            }
        });
    }

    /// <summary>
    /// Configures the application
    /// </summary>
    /// <param name="application">The application</param>
    public void Configure(IApplicationBuilder application)
    {
        application.UseRouting();
        application.UseCors(policyName: "CorsPolicy");
        application.UseAuthorization();
        application.UseSwagger(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
        });

        application.UseSwagger(options =>
        {
            options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });
        application.UseReDoc(c =>
        {
            c.RoutePrefix = "api-docs";
            c.DocumentTitle = "JITS OpenAPI WEB API";
            c.SpecUrl = "v1/swagger.json";
        });
        application.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "api";
            c.SwaggerEndpoint("/api-docs/v1/swagger.json", "JITS OpenAPI Web API");
        });
    }

    /// <summary>
    /// Gets the open api uris
    /// </summary>
    /// <exception cref="InvalidOperationException">URL for webApi endpoint is not configured.</exception>
    /// <exception cref="InvalidOperationException">WebApi endpoint not found in Kestrel configuration.</exception>
    /// <returns>The full uris</returns>
    private static List<string> GetOpenApiUris()
    {
        var kestrelConfig = Singleton<AppSettings>.Instance.Get<Kestrel>();
        var openApiConfig = Singleton<AppSettings>.Instance.Get<O24OpenAPIConfiguration>();
        var fullUris = new List<string>();
        if (kestrelConfig.Endpoints.TryGetValue("webApi", out var webApiEndpoint))
        {
            if (!string.IsNullOrEmpty(webApiEndpoint.Url))
            {
                var url = webApiEndpoint.Url.Replace("*", "localhost");
                fullUris.Add(url);
                var uri = new Uri(url);
                int port = uri.Port;
                Console.WriteLine($"WebApi endpoint URL: {uri}:{port}");
            }
            else
            {
                throw new InvalidOperationException("URL for webApi endpoint is not configured.");
            }
        }
        else
        {
            throw new InvalidOperationException(
                "WebApi endpoint not found in Kestrel configuration."
            );
        }

        string listOfOpenApiUri = openApiConfig.OpenAPIURI;
        if (!string.IsNullOrEmpty(listOfOpenApiUri))
        {
            foreach (var item in listOfOpenApiUri.Split(';'))
            {
                fullUris.Add(item);
            }
        }

        return fullUris;
    }

    /// <summary>
    /// Gets the value of the order
    /// </summary>
    public int Order => 2001;
}
