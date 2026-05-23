using Microsoft.AspNetCore.Builder;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Services.Queue;

namespace O24OpenAPI.Framework.Infrastructure;

/// <summary>
/// The app extension class
/// </summary>
public static class AppExtension
{
    /// <summary>
    /// Initializes the message queue using the specified application
    /// </summary>
    /// <param name="application">The application</param>
    public static void InitializeMessageQueue(this IApplicationBuilder application)
    {
        if (
            Singleton<O24OpenAPIConfiguration>.Instance.YourServiceID.EqualsOrdinalIgnoreCase("WFO")
        )
        {
            return;
        }

        try
        {
            QueueContext.GetInstance();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Queue client initialization failed: " + ex.Message);
        }
    }
}
