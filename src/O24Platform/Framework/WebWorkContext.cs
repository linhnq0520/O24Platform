using O24OpenAPI.Core.Abstractions;
using O24OpenAPI.Core.Domain.Localization;
using O24OpenAPI.Core.Domain.Users;
using O24OpenAPI.Framework.Services;

namespace O24OpenAPI.Framework;

/// <summary>
///
/// </summary>
/// <param name="languageService"></param>
public class WebWorkContext(ILanguageService languageService) : IWorkContext
{
    /// <summary>
    /// The cache transaction ref id
    /// </summary>
    private string _cacheTransactionRefId;

    /// <summary>
    /// The user
    /// </summary>
    private UserContext UserContext { get; set; }

    /// <summary>
    /// The cache working date
    /// </summary>
    private DateTime _cacheWorkingDate;

    /// <summary>
    /// The cached language
    /// </summary>
    private Language _cachedLanguage;

    /// <summary>
    /// The cache user approve
    /// </summary>
    private string _cacheUserApprove;

    /// <summary>
    /// The cache status reverse
    /// </summary>
    private bool _cacheStatusReverse = false;

    /// <summary>
    /// The channel request
    /// </summary>
    private string ChannelRequest { get; set; }

    /// <summary>
    /// The language service
    /// </summary>
    private readonly ILanguageService _languageService = languageService;

    /// <summary>Get the current transaction reference identifiers</summary>
    /// <returns></returns>
    public virtual async Task<string> GetCurrentRefId()
    {
        if (string.IsNullOrEmpty(_cacheTransactionRefId))
        {
            return "";
        }

        await Task.CompletedTask;
        return _cacheTransactionRefId;
    }

    /// <summary>Gets the current user</summary>
    /// <returns></returns>
    public UserContext GetUserContext()
    {
        return UserContext;
    }

    /// <summary>
    /// Sets the current user using the specified user
    /// </summary>
    /// <param name="user">The user</param>
    public void SetUserContext(UserContext userContext = null)
    {
        if (userContext != null)
        {
            UserContext = userContext;
        }
    }

    /// <summary>
    /// Gets the current user approve
    /// </summary>
    /// <returns>A task containing the string</returns>
    public virtual async Task<string> GetCurrentUserApprove()
    {
        if (string.IsNullOrEmpty(_cacheUserApprove))
        {
            return "";
        }

        await Task.CompletedTask;
        return _cacheUserApprove;
    }

    /// <summary>
    /// Gets the status reverse
    /// </summary>
    /// <returns>A task containing the bool</returns>
    public virtual async Task<bool> GetStatusReverse()
    {
        await Task.CompletedTask;
        return _cacheStatusReverse;
    }

    /// <summary>Get the working date</summary>
    /// <param name="reload"></param>
    /// <param name="inBatch">default false: normal for wf,
    /// true: for batch step call to return true working date</param>
    /// <param name="channelId"></param>
    /// <returns></returns>
    public virtual async Task<DateTime> GetWorkingDate(
        bool reload = false,
        bool inBatch = false,
        string channelId = ""
    )
    {
        if (_cacheWorkingDate.Year == 1 | reload)
        {
            try
            {
                //IAdminService adminService = EngineContext.Current.Resolve<IAdminService>();
                //DateTime workingDate = await adminService.GetWorkingDate(inBatch, channelId);
                _cacheWorkingDate = DateTime.UtcNow; //workingDate;
                //adminService = (IAdminService)null;
            }
            catch (Exception)
            {
                //IGrpcService grpcService = EngineContext.Current.Resolve<IGrpcService>();
                //string admin = "ADM";
                //string assemblyName = "O24Admin.Web.Admin.AdminGrpcService";
                //string methodName = "GetBusDateByChannel";
                //string str = await grpcService.CacheCall<string>(
                //    admin,
                //    assemblyName,
                //    methodName,
                //    false,
                //    inBatch.ToString(),
                //    channelId
                //);
                //_cacheWorkingDate = DateTime.Parse(str, null, DateTimeStyles.RoundtripKind);
                //grpcService = null;
                //admin = null;
                //assemblyName = null;
                //methodName = null;
                //str = null;
            }
        }

        return _cacheWorkingDate;
    }

    /// <summary>
    /// Gets the working language
    /// </summary>
    /// <returns>A task containing the language</returns>
    public virtual async Task<Language> GetWorkingLanguage()
    {
        if (_cachedLanguage != null)
        {
            return _cachedLanguage;
        }

        await Task.CompletedTask;
        _cachedLanguage = GetDefaultLanguage();
        return _cachedLanguage;
    }

    /// <summary>
    /// Sets the current ref id using the specified ref id
    /// </summary>
    /// <param name="refId">The ref id</param>
    public virtual async Task SetCurrentRefId(string refId)
    {
        _cacheTransactionRefId = refId;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Sets the current user approve using the specified user approve
    /// </summary>
    /// <param name="userApprove">The user approve</param>
    public virtual async Task SetCurrentUserApprove(string userApprove)
    {
        _cacheUserApprove = userApprove;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Sets the status reverse using the specified is reverse
    /// </summary>
    /// <param name="isReverse">The is reverse</param>
    public virtual async Task SetStatusReverse(bool isReverse)
    {
        _cacheStatusReverse = isReverse;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Sets the working date using the specified working date
    /// </summary>
    /// <param name="workingDate">The working date</param>
    public virtual async Task SetWorkingDate(DateTime workingDate)
    {
        _cacheWorkingDate = workingDate;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Sets the working language using the specified language
    /// </summary>
    /// <param name="language">The language</param>
    public virtual async Task SetWorkingLanguage(Language language)
    {
        await Task.CompletedTask;
        UserContext user = UserContext;
        _cachedLanguage = null;
        user = null;
    }

    /// <summary>
    /// Sets the working language using the specified language
    /// </summary>
    /// <param name="language">The language</param>
    public virtual async Task SetWorkingLanguage(string language)
    {
        Language lang;
        if (language == "en")
        {
            _cachedLanguage = GetDefaultLanguage();
            lang = null;
        }
        else
        {
            lang = await _languageService.GetByCode(language);
            if (lang == null)
            {
                Language userLanguage = GetDefaultLanguage();
                _cachedLanguage = userLanguage;
                userLanguage = null;
                lang = null;
            }
            else
            {
                _cachedLanguage = lang;
                lang = null;
            }
        }
    }

    /// <summary>
    /// Gets the default language
    /// </summary>
    /// <returns>The default language</returns>
    private static Language GetDefaultLanguage()
    {
        Language defaultLanguage = new()
        {
            Id = 1,
            Name = "English",
            LanguageCulture = "en-US",
            UniqueSeoCode = "en",
            DisplayOrder = 1,
        };
        return defaultLanguage;
    }

    /// <summary>
    /// Gets the channel request
    /// </summary>
    /// <returns>The channel request</returns>
    public string GetChannelRequest()
    {
        return ChannelRequest;
    }

    /// <summary>
    /// Sets the channel request using the specified channel request
    /// </summary>
    /// <param name="channelRequest">The channel request</param>
    public void SetChannelRequest(string channelRequest)
    {
        ChannelRequest = channelRequest;
    }
}
