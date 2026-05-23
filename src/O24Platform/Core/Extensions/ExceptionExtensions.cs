using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace O24OpenAPI.Core.Extensions;

/// <summary>
/// The exception extensions class
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Debugs the message using the specified ex
    /// </summary>
    /// <param name="ex">The ex</param>
    /// <param name="frameNumber">The frame number</param>
    /// <returns>The string</returns>
    public static string DebugMessage(this Exception ex, int frameNumber = -1)
    {
        if (ex is not O24OpenAPIException)
            return ex.ToString();

        var stackTrace = new StackTrace(ex, true);

        if (frameNumber < 0 || frameNumber >= stackTrace.FrameCount)
            frameNumber = stackTrace.FrameCount - 1;

        var frame = stackTrace.GetFrame(frameNumber);

        var line = frame?.GetFileLineNumber() ?? 0;
        var typeName = frame?.GetMethod()?.ReflectedType?.FullName ?? "UnknownType";

        return $"{ex.GetType().Name}: {ex.Message}\n{typeName} at line {line}";
    }
}
