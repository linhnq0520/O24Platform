using System.Linq.Expressions;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Domain.Localization;
using O24OpenAPI.Core.SeedWork;

namespace O24OpenAPI.Framework.Services;

/// <summary>Language service</summary>
public class LanguageService : ILanguageService
{
    /// <summary>
    /// The language repository
    /// </summary>
    private readonly IRepository<Language> _languageRepository;

    /// <summary>
    /// The static cache manager
    /// </summary>
    private readonly IStaticCacheManager _staticCacheManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageService"/> class
    /// </summary>
    /// <param name="languageRepository">The language repository</param>
    /// <param name="staticCacheManager">The static cache manager</param>
    public LanguageService(
        IRepository<Language> languageRepository,
        IStaticCacheManager staticCacheManager
    )
    {
        _languageRepository = languageRepository;
        _staticCacheManager = staticCacheManager;
    }

    /// <summary>Gets all language</summary>
    /// <returns></returns>
    public virtual async Task<IList<Language>> GetAll()
    {
        IList<Language> languages = await _languageRepository.GetAll(
            query =>
            {
                ParameterExpression parameterExpression = Expression.Parameter(
                    typeof(Language),
                    "language"
                );
                MemberExpression idProperty = Expression.Property(
                    parameterExpression,
                    nameof(BaseEntity.Id)
                );
                var orderByExpression = Expression.Lambda<Func<Language, int>>(
                    idProperty,
                    parameterExpression
                );
                query = query.OrderBy(orderByExpression);
                return query;
            },
            null
        );

        return languages;
    }

    /// <summary>Gets a language</summary>
    /// <param name="languageId">Language identifier</param>
    /// <returns></returns>
    public virtual async Task<Language> GetById(int languageId)
    {
        Language byId = await _languageRepository.GetById(languageId, cache => null);
        return byId;
    }

    /// <summary>
    /// Gets the by code using the specified code
    /// </summary>
    /// <param name="code">The code</param>
    /// <returns>A task containing the language</returns>
    public virtual async Task<Language> GetByCode(string code)
    {
        await Task.CompletedTask;
        return null;
    }
}
