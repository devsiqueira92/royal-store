using RoyalStore.Domain.Sellers;

namespace RoyalStore.Domain.Customers;

/// <summary>
/// Validates a Brazilian taxpayer document: an 11-digit CPF or a 14-digit CNPJ.
/// CPF validation reuses <see cref="CpfValidator"/>; CNPJ check digits are computed here.
/// </summary>
public static class DocumentValidator
{
    private static readonly int[] CnpjFirstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] CnpjSecondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public static string Normalize(string document) =>
        new(document.Where(char.IsDigit).ToArray());

    public static bool IsValid(string document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return false;

        var digits = Normalize(document);

        return digits.Length switch
        {
            11 => CpfValidator.IsValid(digits),
            14 => IsValidCnpj(digits),
            _ => false
        };
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.All(c => c == cnpj[0]))
            return false;

        var digits = cnpj.Select(c => c - '0').ToArray();

        var firstCheck = ComputeCheckDigit(digits, CnpjFirstWeights);
        if (digits[12] != firstCheck)
            return false;

        var secondCheck = ComputeCheckDigit(digits, CnpjSecondWeights);
        return digits[13] == secondCheck;
    }

    private static int ComputeCheckDigit(int[] digits, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
            sum += digits[i] * weights[i];

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
