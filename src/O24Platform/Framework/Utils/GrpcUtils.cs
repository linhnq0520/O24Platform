using Grpc.Core;

namespace O24OpenAPI.Framework.Utils;

/// <summary>
/// The grpc utils class
/// </summary>
public class GrpcUtils
{
    /// <summary>
    /// Gets the client ip using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <returns>The string</returns>
    public static string GetClientIp(ServerCallContext context)
    {
        var peer = context.Peer;
        if (peer.StartsWith("ipv4:"))
        {
            return peer[5..].Split(':')[0];
        }
        if (peer.StartsWith("ipv6:"))
        {
            return peer[5..].Split('%')[0];
        }
        return "Unknown";
    }

    /// <summary>
    /// Gets the user agent using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <returns>The string</returns>
    public static string GetUserAgent(ServerCallContext context)
    {
        var userAgentEntry = context.RequestHeaders.FirstOrDefault(h => h.Key == "user-agent");
        return userAgentEntry?.Value ?? "Unknown";
    }
}
