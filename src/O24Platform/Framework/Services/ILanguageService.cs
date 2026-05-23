using O24OpenAPI.Core.Domain.Localization;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The language service interface
/// </summary>
public interface ILanguageService
{
    /// <summary>Gets all languages</summary>
    /// <returns></returns>
    Task<IList<Language>> GetAll();

    /// <summary>Gets a language</summary>
    /// <param name="languageId">Language identifier</param>
    /// <returns></returns>
    Task<Language> GetById(int languageId);

    /// <summary>Gets a language</summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<Language> GetByCode(string code);
}
