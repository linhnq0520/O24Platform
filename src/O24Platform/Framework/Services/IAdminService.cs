using O24OpenAPI.Core.Domain.Users;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The admin service interface
/// </summary>
public interface IAdminService
{
    /// <summary>Get working date</summary>
    /// <returns></returns>
    Task<DateTime> GetWorkingDate(bool inBatch = false, string channelId = "");

    /// <summary>
    /// Gets the working date using the specified in batch
    /// </summary>
    /// <param name="inBatch">The in batch</param>
    /// <returns>A task containing the date time</returns>
    Task<DateTime> GetWorkingDate(bool inBatch = false);

    /// <summary>Get user info</summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<User> GetUser(int userId);

    /// <summary>
    /// Gets the transaction number using the specified reference id
    /// </summary>
    /// <param name="referenceId">The reference id</param>
    /// <returns>A task containing the string</returns>
    Task<string> GetTransactionNumber(string referenceId);
}
