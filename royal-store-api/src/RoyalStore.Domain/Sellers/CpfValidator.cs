namespace RoyalStore.Domain.Sellers;

public static class CpfValidator
{
    public static string Normalize(string cpf) =>
        new string(cpf.Where(char.IsDigit).ToArray());

    public static bool IsValid(string cpf)
    {
        cpf = Normalize(cpf);

        if (cpf.Length != 11)
            return false;

        if (cpf.All(c => c == cpf[0]))
            return false;

        var digits = cpf.Select(c => c - '0').ToArray();

        int firstSum = 0;
        for (int i = 0; i < 9; i++)
            firstSum += digits[i] * (10 - i);

        int firstRemainder = firstSum % 11;
        int firstCheck = firstRemainder < 2 ? 0 : 11 - firstRemainder;

        if (digits[9] != firstCheck)
            return false;

        int secondSum = 0;
        for (int i = 0; i < 10; i++)
            secondSum += digits[i] * (11 - i);

        int secondRemainder = secondSum % 11;
        int secondCheck = secondRemainder < 2 ? 0 : 11 - secondRemainder;

        return digits[10] == secondCheck;
    }
}
