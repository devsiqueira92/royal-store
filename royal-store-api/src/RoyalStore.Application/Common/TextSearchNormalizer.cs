using System.Globalization;
using System.Text;

namespace RoyalStore.Application.Common;

/// <summary>
/// Normalizes free text for accent- and punctuation-insensitive matching:
/// decomposes diacritics, drops combining marks, and keeps only lowercase [a-z0-9].
/// The database mirror is the scalar function <c>dbo.fn_NormalizeSearchText</c>.
/// </summary>
public static class TextSearchNormalizer
{
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var decomposed = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            var lower = char.ToLowerInvariant(c);
            if (lower is >= 'a' and <= 'z' or >= '0' and <= '9')
                sb.Append(lower);
        }

        return sb.ToString();
    }
}
