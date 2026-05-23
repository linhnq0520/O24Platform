using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data.System.Linq;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The stored command service class
/// </summary>
/// <seealso cref="IStoredCommandService"/>
public class StoredCommandService(IRepository<StoredCommand> contextRepository)
    : IStoredCommandService
{
    /// <summary>
    /// The context repository
    /// </summary>
    private readonly IRepository<StoredCommand> _contextRepository = contextRepository;

    public virtual async Task<StoredCommand> GetByIdAsync(int id)
    {
        return await _contextRepository.GetById(id, cache => null);
    }

    public virtual async Task<IList<StoredCommand>> GetAllAsync()
    {
        return await _contextRepository.GetAll(query => query);
    }

    public async Task UpdateAsync(StoredCommand storedCommand)
    {
        await _contextRepository.Update(storedCommand);
    }

    public async Task<StoredCommand> AddAsync(StoredCommand storedCommand)
    {
        return await _contextRepository.InsertAsync(storedCommand);
    }

    public async Task DeleteAsync(StoredCommand storedCommand)
    {
        await _contextRepository.InsertAsync(storedCommand);
    }

    public async Task<IPagedList<StoredCommand>> SimpleSearch(SimpleSearchModel model)
    {
        var query =
            from d in _contextRepository.Table
            where
                (!string.IsNullOrEmpty(model.SearchText) && d.Name.Contains(model.SearchText))
                || true
            select d;
        return await query.ToPagedList(model.PageIndex, model.PageSize);
    }

    /// <summary>
    /// Gets the by name using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The command</returns>
    public async Task<StoredCommand> GetByName(string name)
    {
        var command = await _contextRepository
            .Table.Where(s => s.Name == name)
            .FirstOrDefaultAsync();
        return command;
    }
}
