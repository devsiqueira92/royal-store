namespace RoyalStore.Application.Common.Pagination;

/// <summary>
/// Cursor-paged (keyset) list request shared by every list (067): <see cref="Cursor"/> is the opaque
/// token from the previous page's <c>NextCursor</c> (<c>null</c> → first page); <see cref="PageSize"/>
/// defaults to <see cref="DefaultPageSize"/> and is capped at <see cref="MaxPageSize"/>.
/// </summary>
public sealed record PageRequest(string? Cursor = null, int? PageSize = null)
{
    public const int DefaultPageSize = 30;
    public const int MaxPageSize = 100;

    /// <summary>The page size to use, once validated by <see cref="PageRequestValidator"/>.</summary>
    public int Size => PageSize ?? DefaultPageSize;
}
