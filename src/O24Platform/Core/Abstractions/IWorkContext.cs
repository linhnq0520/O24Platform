using O24OpenAPI.Core.Domain.Localization;
using O24OpenAPI.Core.Domain.Users;

namespace O24OpenAPI.Core.Abstractions;

/// <summary>
/// The work context interface
/// </summary>
public interface IWorkContext
{
    /// <summary>
    /// Gets the current user
    /// </summary>
    /// <returns>A task containing the user</returns>
    UserContext GetUserContext();

    void SetUserContext(UserContext? userContext = null);

    /// <summary>
    /// Gets the working language
    /// </summary>
    /// <returns>A task containing the language</returns>
    Task<Language> GetWorkingLanguage();

    /// <summary>
    /// Sets the working language using the specified language
    /// </summary>
    /// <param name="language">The language</param>
    Task SetWorkingLanguage(Language language);

    /// <summary>
    /// Sets the working language using the specified language
    /// </summary>
    /// <param name="language">The language</param>
    Task SetWorkingLanguage(string language);

    /// <summary>
    /// Sets the current ref id using the specified ref id
    /// </summary>
    /// <param name="refId">The ref id</param>
    Task SetCurrentRefId(string refId);

    /// <summary>
    /// Gets the current ref id
    /// </summary>
    /// <returns>A task containing the string</returns>
    Task<string> GetCurrentRefId();

    /// <summary>
    /// Sets the current user approve using the specified user approve
    /// </summary>
    /// <param name="userApprove">The user approve</param>
    Task SetCurrentUserApprove(string userApprove);

    /// <summary>
    /// Gets the current user approve
    /// </summary>
    /// <returns>A task containing the string</returns>
    Task<string> GetCurrentUserApprove();

    /// <summary>
    /// Sets the status reverse using the specified is reverse
    /// </summary>
    /// <param name="isReverse">The is reverse</param>
    Task SetStatusReverse(bool isReverse);

    /// <summary>
    /// Gets the status reverse
    /// </summary>
    /// <returns>A task containing the bool</returns>
    Task<bool> GetStatusReverse();

    /// <summary>
    /// Sets the working date using the specified working date
    /// </summary>
    /// <param name="workingDate">The working date</param>
    Task SetWorkingDate(DateTime workingDate);

    /// <summary>
    /// Gets the working date using the specified reload
    /// </summary>
    /// <param name="reload">The reload</param>
    /// <param name="inBatch">The in batch</param>
    /// <param name="channelId">The channel id</param>
    /// <returns>A task containing the date time</returns>
    Task<DateTime> GetWorkingDate(bool reload = false, bool inBatch = false, string channelId = "");

    /// <summary>
    /// Gets the channel request
    /// </summary>
    /// <returns>The string</returns>
    string GetChannelRequest();

    /// <summary>
    /// Sets the channel request using the specified channel request
    /// </summary>
    /// <param name="channelRequest">The channel request</param>
    void SetChannelRequest(string channelRequest);
}
