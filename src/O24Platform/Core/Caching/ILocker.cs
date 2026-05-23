namespace O24OpenAPI.Core.Caching;

/// <summary>
/// The locker interface
/// </summary>
public interface ILocker
{
    /// <summary>
    /// Describes whether this instance perform action with lock
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="expirationTime">The expiration time</param>
    /// <param name="action">The action</param>
    /// <returns>The bool</returns>
    bool PerformActionWithLock(string resource, TimeSpan expirationTime, Action action);
}
