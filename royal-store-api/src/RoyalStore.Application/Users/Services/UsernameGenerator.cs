using System.Globalization;
using System.Text;

namespace RoyalStore.Application.Users.Services;

public static class UsernameGenerator
{
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            var lower = char.ToLowerInvariant(c);
            if (lower >= 'a' && lower <= 'z' || lower >= '0' && lower <= '9')
                sb.Append(lower);
        }

        return sb.ToString();
    }

    public static string GenerateBase(string firstName, string lastName, string storeHandle) =>
        $"{Normalize(firstName)}.{Normalize(lastName)}@{Normalize(storeHandle)}";
}
