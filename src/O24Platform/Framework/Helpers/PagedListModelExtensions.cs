using LinqToDB;
using LinqToDB.Async;
using O24OpenAPI.Core;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Helpers;

/// <summary>
/// The model extensions class
/// </summary>
public static class PagedListModelExtensions
{
    /// <summary>
    /// Returns the paged list model using the specified items
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <typeparam name="T">The </typeparam>
    /// <param name="items">The items</param>
    /// <returns>A paged list model of t entity and t</returns>
    public static PagedListModel<TEntity, T> ToPagedListModel<TEntity, T>(
        this IPagedList<TEntity> items
    )
        where T : BaseO24OpenAPIModel
    {
        return new PagedListModel<TEntity, T>(items);
    }

    /// <summary>
    /// To the paged list model async
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="mapper"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<PagedListModel<TModel>> ToPagedListModelAsync<T, TModel>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        Func<IReadOnlyList<T>, List<TModel>> mapper,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(mapper);

        pageIndex = pageIndex <= 0 ? 1 : pageIndex;

        const int DefaultPageSize = 50;
        const int MaxPageSize = int.MaxValue;

        pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
        pageSize = Math.Min(pageSize, MaxPageSize);

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var modelItems = mapper(items);

        return new PagedListModel<TModel>(modelItems, pageIndex, pageSize, totalCount);
    }

    public static async Task<PagedListModel<TModel>> ToPagedListModelAsync<TModel>(
        this IQueryable<TModel> query,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        pageIndex = pageIndex <= 0 ? 1 : pageIndex;

        const int DefaultPageSize = 50;
        const int MaxPageSize = int.MaxValue;

        pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
        pageSize = Math.Min(pageSize, MaxPageSize);

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedListModel<TModel>(items, pageIndex, pageSize, totalCount);
    }

    public static PagedListModel<TModel> ToPagedListModel<T, TModel>(
        this IPagedList<T> pagedList,
        Func<IReadOnlyList<T>, List<TModel>> mapper
    )
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var modelItems = mapper([.. pagedList]);

        return new PagedListModel<TModel>(
            modelItems,
            pagedList.PageIndex,
            pagedList.PageSize,
            pagedList.TotalCount
        );
    }
}
