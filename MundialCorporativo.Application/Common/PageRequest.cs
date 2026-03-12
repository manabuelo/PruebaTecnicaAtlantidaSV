namespace MundialCorporativo.Application.Common;

public record PageRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null)
{
    public int SafePageNumber => PageNumber <= 0 ? 1 : PageNumber;
    public int SafePageSize => PageSize is <= 0 or > 100 ? 10 : PageSize;
    public int Offset => (SafePageNumber - 1) * SafePageSize;
    public string SafeSortDirection => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
}
