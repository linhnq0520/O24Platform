using System.Collections;

namespace O24OpenAPI.Core;

/// <summary>
/// The paged list interface
/// </summary>
/// <seealso cref="IList{T}"/>
/// <seealso cref="ICollection{T}"/>
/// <seealso cref="IEnumerable{T}"/>
/// <seealso cref="IEnumerable"/>
public interface IPagedList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
    /// <summary>
    /// Gets the value of the page index
    /// </summary>
    int PageIndex { get; }

    /// <summary>
    /// Gets the value of the page size
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets the value of the total count
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Gets the value of the total pages
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Gets the value of the has previous page
    /// </summary>
    bool HasPreviousPage { get; }

    /// <summary>
    /// Gets the value of the has next page
    /// </summary>
    bool HasNextPage { get; }
    /// <summary>
    /// Gets the value of the total success
    /// </summary>
    int TotalSuccess { get; }
    /// <summary>
    /// Get the value of the total failed
    /// </summary>
    int TotalFailed { get; }
}
