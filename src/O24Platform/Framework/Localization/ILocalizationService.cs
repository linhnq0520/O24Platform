using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain.Localization;

namespace O24OpenAPI.Framework.Localization;

/// <summary>
/// The localization service interface
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<IPagedList<LocaleStringResource>> GetAll(
        string resource = "",
        int pageIndex = 0,
        int pageSize = 2147483647
    );

    /// <summary>Gets a locale string resource</summary>
    /// <param name="localeStringResourceId">Locale string resource identifier</param>
    /// <returns>The task result contains the locale string resource</returns>
    Task<LocaleStringResource> GetById(int localeStringResourceId);

    /// <summary>Gets a locale string resource</summary>
    /// <param name="resourceName">A string representing a resource name</param>
    /// <param name="language">Language</param>
    /// <returns></returns>
    Task<LocaleStringResource> GetByName(string resourceName, string language);

    /// <summary>
    /// Gets a resource string based on the specified ResourceKey property
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <returns></returns>
    Task<string> GetResource(string resourceKey);

    /// <summary>
    /// Gets a resource string based on the specified ResourceKey property
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <param name="language"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    Task<string> GetResource(string resourceKey, string language, string defaultValue = "");

    /// <summary>Add locale resources</summary>
    /// <param name="resources"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    Task AddLocaleResource(IDictionary<string, string> resources, string language = null);

    /// <summary>Delete locale resources</summary>
    /// <param name="resourceNames"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    Task DeleteLocaleResources(IList<string> resourceNames, string language = null);
}
