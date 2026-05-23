using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Framework.Infrastructure.Extensions;

/// <summary>
/// The web application builder extensions class
/// </summary>
public static partial class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the web host using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    public static void ConfigureWebHost(this WebApplicationBuilder builder)
    {
        var kestrelConfig = Singleton<AppSettings>.Instance.Get<Kestrel>();
        var usedPorts = new HashSet<int>();

        builder.WebHost.ConfigureKestrel(options =>
        {
            foreach (var endpoint in kestrelConfig.Endpoints)
            {
                var url = Environment.ExpandEnvironmentVariables(
                    endpoint.Value.Url ?? string.Empty
                );
                Match match = URLRegex().Match(url);

                if (match.Success && int.TryParse(match.Groups[1].Value, out int port))
                {
                    Console.WriteLine($"Port: {port}");
                }
                else
                {
                    Console.WriteLine($"Invalid URL format: {url}");
                    continue;
                }

                if (!usedPorts.Add(port))
                {
                    Console.WriteLine($"[SKIP] Port {port} already bound. Skipping duplicate.");
                    continue;
                }
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Binding port {port}...");
                options.ListenAnyIP(
                    port,
                    listenOptions =>
                    {
                        listenOptions.Protocols = endpoint.Value.Protocols switch
                        {
                            "Http2" => HttpProtocols.Http2,
                            "Http1AndHttp2" => HttpProtocols.Http1AndHttp2,
                            _ => HttpProtocols.Http1,
                        };

                        if (endpoint.Value.Certificate != null)
                        {
                            listenOptions.UseHttps(
                                endpoint.Value.Certificate.Path,
                                endpoint.Value.Certificate.Password
                            );
                        }
                    }
                );
            }
        });
    }

    /// <summary>
    /// Mys the regex
    /// </summary>
    /// <returns>The regex</returns>
    [GeneratedRegex(@":(\d+)$")]
    private static partial Regex URLRegex();
}
