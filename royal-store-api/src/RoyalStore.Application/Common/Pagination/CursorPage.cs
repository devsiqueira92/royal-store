using RoyalStore.Application.DTOs;

namespace RoyalStore.Application.Common.Pagination;

public static class CursorPage
{
    /// <summary>
    /// Builds a page from rows fetched with <c>Take(pageSize + 1)</c>: the extra row only tells whether
    /// more exist (067 FR-008, no COUNT), and the cursor points at the last row actually returned.
    /// </summary>
    public static CursorPageResponse<TItem> Create<TRow, TItem, TKey>(
        IReadOnlyList<TRow> rows, int pageSize, Func<TRow, TKey> keyOf, Func<TRow, TItem> map)
        where TKey : class
    {
        var hasMore = rows.Count > pageSize;
        var pageRows = hasMore ? rows.Take(pageSize).ToList() : rows;
        var nextCursor = hasMore ? CursorCodec.Encode(keyOf(pageRows[^1])) : null;

        return new CursorPageResponse<TItem>(pageRows.Select(map).ToList(), nextCursor, hasMore);
    }
}
