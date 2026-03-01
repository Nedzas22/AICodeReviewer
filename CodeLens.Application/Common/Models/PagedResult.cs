namespace CodeLens.Application.Common.Models;

/// <summary>
/// Generic wrapper for paged query results.
/// Carries both the current page of data and the metadata needed to render pagination controls.
/// </summary>
/// <typeparam name="T">The DTO type for each item in the page.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>Gets the items for the current page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Gets the total number of matching records across all pages.</summary>
    public int TotalCount { get; }

    /// <summary>Gets the current 1-based page index.</summary>
    public int Page { get; }

    /// <summary>Gets the maximum number of items per page.</summary>
    public int PageSize { get; }

    /// <summary>Gets the computed total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Gets a value indicating whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Gets a value indicating whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Initialises a new <see cref="PagedResult{T}"/>.
    /// </summary>
    /// <param name="items">The items for this page.</param>
    /// <param name="totalCount">The total record count (all pages).</param>
    /// <param name="page">The current 1-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
