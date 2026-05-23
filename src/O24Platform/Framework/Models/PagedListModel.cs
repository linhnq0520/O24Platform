using O24OpenAPI.Core;
using O24OpenAPI.Framework.Infrastructure.Mapper.Extensions;

namespace O24OpenAPI.Framework.Models;

public class PagedListModel<TEntity, T> : PagedModel
    where T : BaseO24OpenAPIModel
{
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
    public List<T> Items { get; set; } = [];
    public int? TotalSuccess { get; set; } = 0;
    public int? TotalFailed { get; set; } = 0;

    public PagedListModel() { }

    public PagedListModel(IPagedList<TEntity> items)
    {
        PageIndex = items.PageIndex;
        PageSize = items.PageSize;
        TotalCount = items.TotalCount;
        TotalPages = items.TotalPages;
        HasPreviousPage = items.HasPreviousPage;
        HasNextPage = items.HasNextPage;
        Items.AddRange(items.Select(p => p.ToModel<T>()));
        TotalSuccess = items.TotalSuccess;
        TotalFailed = items.TotalFailed;
    }

    public PagedListModel(List<T> items, int pageIndex, int pageSize, int totalCount)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = items.Count;
        this.TotalPages = items.Count / pageSize;
        if (this.TotalCount % pageSize > 0)
        {
            this.TotalPages++;
        }
        HasPreviousPage = pageIndex + 1 < TotalPages;
        HasNextPage = pageIndex > 0;
        Items = [.. items];
    }
}

public class PagedListModel<T> : PagedModel
{
    public int TotalCount { get; set; } = 0;
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public List<T> Items { get; set; } = [];
    public int? TotalSuccess { get; set; } = 0;
    public int? TotalFailed { get; set; } = 0;

    public PagedListModel() { }

    public PagedListModel(IPagedList<T> items)
    {
        PageIndex = items.PageIndex;
        PageSize = items.PageSize;
        TotalCount = items.TotalCount;
        TotalPages = items.TotalPages;
        HasPreviousPage = items.HasPreviousPage;
        HasNextPage = items.HasNextPage;
        Items = [.. items];
        TotalSuccess = items.TotalSuccess;
        TotalFailed = items.TotalFailed;
    }

    public PagedListModel(List<T> items, int pageIndex, int pageSize, int totalCount)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
        this.TotalPages = totalCount / pageSize;
        if (this.TotalCount % pageSize > 0)
        {
            this.TotalPages++;
        }
        HasPreviousPage = pageIndex + 1 < TotalPages;
        HasNextPage = pageIndex > 0;
        Items = [.. items];
    }
}
