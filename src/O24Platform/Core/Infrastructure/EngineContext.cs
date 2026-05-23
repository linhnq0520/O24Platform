using System.Runtime.CompilerServices;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// The engine context class
/// /// </summary>
public class EngineContext
{
    /// <summary>
    /// Creates
    /// </summary>
    /// <returns>The engine</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static IEngine Create()
    {
        return Singleton<IEngine>.Instance
            ?? (Singleton<IEngine>.Instance = new O24OpenAPIEngine());
    }

    /// <summary>
    /// Replaces the engine
    /// </summary>
    /// <param name="engine">The engine</param>
    public static void Replace(IEngine engine) => Singleton<IEngine>.Instance = engine;

    /// <summary>
    /// Gets the value of the current
    /// </summary>
    public static IEngine Current
    {
        get
        {
            if (Singleton<IEngine>.Instance == null)
            {
                EngineContext.Create();
            }

            return Singleton<IEngine>.Instance!;
        }
    }
}
