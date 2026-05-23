using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Infrastructure;

namespace O24OpenAPI.Framework.Extensions;

public static class StartupExtensions
{
    public static void ShowStartupBanner(this IApplicationBuilder app)
    {
        var sp = app.ApplicationServices;

        var env = sp.GetRequiredService<IHostEnvironment>();
        var config = sp.GetRequiredService<IConfiguration>();

        var applicationName = env.ApplicationName;
        var environmentName = env.EnvironmentName;

        var version = GetAppVersion();
        var machine = Environment.MachineName;
        var os = RuntimeInformation.OSDescription;
        var framework = RuntimeInformation.FrameworkDescription;
        var kestrelConfig = Singleton<AppSettings>.Instance.Get<Kestrel>();
        var urls = "not set";
        if (kestrelConfig is not null)
        {
            urls = string.Join(";", kestrelConfig.Endpoints.Select(e => e.Value.Url));
        }
        var buildDate = GetAssemblyMetadata("BuildDate") ?? "unknown";
        var buildUser = GetAssemblyMetadata("BuildUser") ?? "unknown";
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        PrintLogo();

        PrintDetails(
            applicationName,
            environmentName,
            version,
            framework,
            machine,
            os,
            urls,
            buildDate,
            buildUser,
            timestamp
        );
    }

    private static string GetAppVersion()
    {
        var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (!string.IsNullOrWhiteSpace(info?.InformationalVersion))
        {
            return info.InformationalVersion!;
        }

        return asm.GetName().Version?.ToString() ?? "unknown";
    }

    private static string GetAssemblyMetadata(string key)
    {
        var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        return asm.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => string.Equals(a.Key, key, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }

    private static void PrintLogo()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(
            @"
************************************************************
*                                                          *
*      /$$$$$$   /$$$$$$  /$$   /$$                        *
*     /$$__  $$ /$$__  $$| $$  | $$                        *
*    | $$  \ $$|__/  \ $$| $$  | $$                        *
*    | $$  | $$  /$$$$$$/| $$$$$$$$                        *
*    | $$  | $$ /$$____/ |_____  $$                        *
*    | $$  | $$| $$            | $$                        *
*    |  $$$$$$/| $$$$$$$$      | $$                        *
*     \______/ |________/      |__/                        *
*                                                          *
*                     Welcome to O24OpenAPI                *
*                                                          *
************************************************************
"
        );
        Console.ResetColor();
    }

    private static void PrintDetails(
        string applicationName,
        string environmentName,
        string version,
        string framework,
        string machine,
        string os,
        string urls,
        string buildDate,
        string buildUser,
        string timestamp
    )
    {
        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.WriteLine("------------------------------------------------------------");

        PrintKeyValue("Application", applicationName);
        PrintKeyValue("Environment", environmentName);
        PrintKeyValue("Version", version);
        PrintKeyValue("Runtime", framework);
        PrintKeyValue("Machine", machine);
        PrintKeyValue("OS", os);
        PrintKeyValue("URL(s)", urls);
        PrintKeyValue("Build Time", buildDate);
        PrintKeyValue("Built By", buildUser);
        PrintKeyValue("Timestamp", timestamp);

        Console.WriteLine("------------------------------------------------------------");

        PrintKeyValue("Service Status", "RUNNING");
        PrintKeyValue("Queue Status", "READY");
        PrintKeyValue("Logging", "ENABLED");

        Console.WriteLine("------------------------------------------------------------");

        Console.ResetColor();
    }

    private static void PrintKeyValue(string key, string value)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"   {key, -14}: "); // key cố định 14 ký tự

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(value);
    }
}
