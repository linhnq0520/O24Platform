using Microsoft.AspNetCore.Mvc;
using O24OpenAPI.Core.Caching;

namespace O24OpenAPI.Framework.Controllers;

/// <summary>
/// The cache controller class
/// </summary>
/// <seealso cref="BaseController"/>
public class CacheController(IStaticCacheManager staticCacheManager) : BaseController
{
    /// <summary>
    /// The static cache manager
    /// </summary>
    private readonly IStaticCacheManager _staticCacheManager = staticCacheManager;

    /// <summary>
    /// Clears the cache
    /// </summary>
    /// <returns>A task containing the action result</returns>
    [HttpPost]
    public virtual async Task<IActionResult> ClearCache()
    {
        await _staticCacheManager.Clear();
        return Ok("Successful!");
    }

    /// <summary>
    /// Clears the all using the specified admin password
    /// </summary>
    /// <param name="adminPassword">The admin password</param>
    /// <returns>A task containing the action result</returns>
    [HttpPost]
    public virtual async Task<IActionResult> ClearAll(string adminPassword)
    {
        if (adminPassword != "admin")
        {
            return Unauthorized();
        }

        await _staticCacheManager.ClearAll();
        return Ok("Successful!");
    }
}
