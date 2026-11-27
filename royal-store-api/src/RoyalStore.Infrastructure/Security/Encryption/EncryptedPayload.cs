namespace RoyalStore.Infrastructure.Security.Encryption;

/// <summary>Wire format of a ciphertext: <c>v{version}:{base64(nonce ‖ tag ‖ ciphertext)}</c>.</summary>
internal static class EncryptedPayload
{
    public const int NonceSize = 12;
    public const int TagSize = 16;
    public const int HeaderSize = NonceSize + TagSize;

    public static string Prefix(int version) => $"v{version}:";

    /// <summary>True when <paramref name="value"/> starts with a well-formed <c>v{digits}:</c>
    /// prefix. Says nothing about whether the body decrypts.</summary>
    public static bool TryParse(string value, out int version, out string body)
    {
        version = 0;
        body = string.Empty;

        if (value.Length < 3 || value[0] != 'v')
            return false;

        var colon = value.IndexOf(':', 1);
        if (colon < 2 || !int.TryParse(value.AsSpan(1, colon - 1), System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out version) || version < 1)
            return false;

        body = value[(colon + 1)..];
        return true;
    }
}
