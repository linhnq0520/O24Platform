using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Async;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Data.System.Linq;

/// <summary>
/// The async queryable extensions class
/// </summary>
public static class AsyncIQueryableExtensions
{
    /// <summary>
    /// Alls the source
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <param name="source">The source</param>
    /// <param name="predicate">The predicate</param>
    /// <returns>A task containing the bool</returns>
    public static Task<bool> AllAsync<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return AsyncExtensions.AllAsync<TSource>(source, predicate);
    }

    /// <summary>
    /// Firsts the or default using the specified source
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <param name="source">The source</param>
    /// <param name="predicate">The predicate</param>
    /// <returns>A task containing the source</returns>
    public static Task<TSource> FirstOrDefaultAsync<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate = null
    )
    {
        return predicate == null
            ? AsyncExtensions.FirstOrDefaultAsync<TSource>(source)
            : AsyncExtensions.FirstOrDefaultAsync<TSource>(source, predicate);
    }

    /// <summary>
    /// Mins the source
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <param name="source">The source</param>
    /// <returns>A task containing the source</returns>
    public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source)
    {
        return AsyncExtensions.MinAsync<TSource>(source);
    }

    /// <summary>
    /// Mins the source
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TResult">The result</typeparam>
    /// <param name="source">The source</param>
    /// <param name="predicate">The predicate</param>
    /// <returns>A task containing the result</returns>
    public static Task<TResult> MinAsync<TSource, TResult>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TResult>> predicate
    )
    {
        return AsyncExtensions.MinAsync<TSource, TResult>(source, predicate);
    }

    /// <summary>
    /// Singles the source
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <param name="source">The source</param>
    /// <param name="predicate">The predicate</param>
    /// <returns>A task containing the source</returns>
    public static Task<TSource> SingleAsync<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate = null
    )
    {
        return predicate == null
            ? AsyncExtensions.SingleAsync<TSource>(source)
            : AsyncExtensions.SingleAsync<TSource>(source, predicate);
    }

    /// <summary>
    /// Singles the or default using the specified source
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <param name="source">The source</param>
    /// <param name="predicate">The predicate</param>
    /// <returns>A task containing the source</returns>
    public static Task<TSource> SingleOrDefaultAsync<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate = null
    )
    {
        return predicate == null
            ? AsyncExtensions.SingleOrDefaultAsync<TSource>(source)
            : AsyncExtensions.SingleOrDefaultAsync<TSource>(source, predicate);
    }

    /// <summary>
    /// Returns the list using the specified source
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <param name="source">The source</param>
    /// <returns>A task containing a list of t source</returns>
    public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
    {
        return AsyncExtensions.ToListAsync<TSource>(source);
    }

    /// <summary>
    /// Returns the paged list using the specified source
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="source">The source</param>
    /// <param name="pageIndex">The page index</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="getOnlyTotalCount">The get only total count</param>
    /// <returns>A task containing a paged list of t</returns>
    public static async Task<IPagedList<T>> ToPagedList<T>(
        this IQueryable<T> source,
        int pageIndex,
        int pageSize,
        bool getOnlyTotalCount = false,
        int? totalSuccess = 0,
        int? totalFailed = 0
    )
    {
        if (source == null)
        {
            return new PagedList<T>([], pageIndex, pageSize, totalSuccess, totalFailed);
        }

        if (pageSize == 0)
        {
            pageSize = int.MaxValue;
        }

        pageSize = Math.Max(pageSize, 1);
        int count = await source.CountAsync<T>(new CancellationToken());
        List<T> data = [];
        if (!getOnlyTotalCount)
        {
            List<T> objList = data;
            List<T> collection = await Queryable
                .Skip<T>(source, pageIndex * pageSize)
                .Take<T>(pageSize)
                .ToListAsync<T>();
            objList.AddRange(collection);
            objList = null;
            collection = null;
        }
        return new PagedList<T>(
            data,
            pageIndex,
            pageSize,
            new int?(count),
            totalSuccess,
            totalFailed
        );
    }

    /// <summary>
    /// Searches the query
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <param name="query">The query</param>
    /// <param name="propName">The prop name</param>
    /// <param name="searchText">The search text</param>
    /// <returns>The query</returns>
    public static IQueryable<TSource> Search<TSource>(
        this IQueryable<TSource> query,
        string propName,
        string searchText
    )
        where TSource : BaseEntity
    {
        if (query == null || string.IsNullOrEmpty(searchText))
        {
            return query;
        }

        query = query.Where<TSource>(
            (Expression<Func<TSource, bool>>)(
                e =>
                    Sql.Property<string>(e, propName)
                        .Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            )
        );
        return query;
    }

    public static async Task<List<T>> ToListAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default
    )
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;

        if (pageSize <= 0)
            return await query.ToListAsync(ct);

        int skip = (pageIndex - 1) * pageSize;

        return await query.Skip(skip).Take(pageSize).ToListAsync(ct);
    }
}
