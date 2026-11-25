namespace RoyalStore.Application.DTOs;

/// <summary>
/// One page of a cursor-paged list (067), shared by every list. <see cref="NextCursor"/> is sent back
/// as <c>cursor</c> to get the next page; it is <c>null</c> exactly when <see cref="HasMore"/> is false.
/// </summary>
public sealed record CursorPageResponse<T>(IReadOnlyList<T> Items, string? NextCursor, bool HasMore);
