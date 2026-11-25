using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Common.Pagination;

public static class PaginationErrors
{
    /// <summary>The cursor is not one this list issued: malformed, tampered with or from another list (067 FR-006).</summary>
    public static readonly Error InvalidCursor =
        Error.Validation("pagination.cursor_invalid", "The 'cursor' parameter is not valid for this list.");
}
