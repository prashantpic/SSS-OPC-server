namespace Opc.System.Shared.Kernel.Contracts.Common;

/// <summary>
/// A generic DTO to encapsulate a paginated list of items.
/// </summary>
/// <typeparam name="T">The type of the items in the list.</typeparam>
public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; }
    public long TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResultDto(IReadOnlyList<T> items, long totalCount, int pageNumber, int pageSize)
    {
        Items = items ?? new List<T>();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}