using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The stored command service interface
/// </summary>
public interface IStoredCommandService
{
    Task<StoredCommand> GetByIdAsync(int id);

    Task<IList<StoredCommand>> GetAllAsync();

    Task<StoredCommand> AddAsync(StoredCommand storedCommand);
    Task UpdateAsync(StoredCommand storedCommand);
    Task DeleteAsync(StoredCommand storedCommand);
    Task<IPagedList<StoredCommand>> SimpleSearch(SimpleSearchModel model);

    /// <summary>
    /// Gets the by name using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>A task containing the stored command</returns>
    Task<StoredCommand> GetByName(string name);
}
