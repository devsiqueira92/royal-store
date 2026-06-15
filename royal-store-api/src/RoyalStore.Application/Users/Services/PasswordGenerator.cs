using System.Security.Cryptography;

namespace RoyalStore.Application.Users.Services;

public static class PasswordGenerator
{
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*";
    private const string AllChars = Uppercase + Lowercase + Digits + Symbols;
    private const int Length = 16;

    public static string Generate()
    {
        var password = new char[Length];

        // Guarantee at least one of each required character class
        password[0] = PickRandom(Uppercase);
        password[1] = PickRandom(Lowercase);
        password[2] = PickRandom(Digits);
        password[3] = PickRandom(Symbols);

        for (var i = 4; i < Length; i++)
            password[i] = PickRandom(AllChars);

        // Shuffle to avoid predictable positions
        RandomNumberGenerator.Shuffle(password.AsSpan());

        return new string(password);
    }

    private static char PickRandom(string chars) =>
        chars[RandomNumberGenerator.GetInt32(chars.Length)];
}
