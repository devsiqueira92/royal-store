using System.Buffers.Text;
using System.Text.Json;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Common.Pagination;

/// <summary>
/// Turns a list's keyset key (the sort columns of the last item delivered) into the opaque cursor
/// clients echo back, and back again (067 FR-003). Base64url of the key's JSON: URL-safe, and opaque
/// enough that clients do not build it — it is not a security boundary, the query filters still apply.
/// </summary>
public static class CursorCodec
{
    public static string Encode<TKey>(TKey key) where TKey : class =>
        Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(key));

    /// <summary><c>null</c>/blank → no cursor (first page); anything undecodable → <see cref="PaginationErrors.InvalidCursor"/>.</summary>
    public static Result<TKey?> Decode<TKey>(string? cursor) where TKey : class
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return Result<TKey?>.Success(null);

        try
        {
            var key = JsonSerializer.Deserialize<TKey>(Base64Url.DecodeFromChars(cursor));
            return key is null
                ? Result<TKey?>.Failure(PaginationErrors.InvalidCursor)
                : Result<TKey?>.Success(key);
        }
        catch (Exception e) when (e is FormatException or JsonException or NotSupportedException)
        {
            return Result<TKey?>.Failure(PaginationErrors.InvalidCursor);
        }
    }
}
