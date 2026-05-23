using System.Collections;

namespace O24OpenAPI.Core.Domain;

public class PagedList<T>
    : List<T>,
        IPagedList<T>,
        IList<T>,
        ICollection<T>,
        IEnumerable<T>,
        IEnumerable
{
    public PagedList(
        IList<T> source,
        int pageIndex,
        int pageSize,
        int? totalCount = null,
        int? totalSuccess = null,
        int? totalFailed = null
    )
    {
        this.TotalCount = totalCount ?? source.Count;
        this.TotalPages = this.TotalCount / pageSize;
        if (this.TotalCount % pageSize > 0)
        {
            this.TotalPages++;
        }

        this.PageSize = pageSize;
        this.PageIndex = pageIndex;
        this.AddRange(
            totalCount.HasValue ? source : source.Skip<T>(pageIndex * pageSize).Take<T>(pageSize)
        );
        this.TotalSuccess = totalSuccess ?? 0;
        this.TotalFailed = totalFailed ?? 0;
    }

    /// <summary>
    /// Gets the value of the page index
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Gets the value of the page size
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the value of the total count
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the value of the total pages
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Gets the value of the has previous page
    /// </summary>
    public bool HasPreviousPage => this.PageIndex > 0;

    /// <summary>
    /// Gets the value of the has next page
    /// </summary>
    public bool HasNextPage => this.PageIndex + 1 < this.TotalPages;

    /// <summary>
    /// Get or set total success
    /// </summary>
    public int TotalSuccess { get; }

    /// <summary>
    /// Get or set total failed
    /// </summary>
    public int TotalFailed { get; }
}
