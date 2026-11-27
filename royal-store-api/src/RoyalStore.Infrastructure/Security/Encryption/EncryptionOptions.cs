using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Infrastructure.Security.Encryption;

/// <summary>
/// Bound from the <c>Encryption</c> section. Keys are Base64 and must come from environment
/// variables or a secret store (<c>Encryption__MasterKey</c>, <c>Encryption__BlindIndexKey</c>) —
/// never from a committed appsettings file.
/// </summary>
public sealed class EncryptionOptions : IValidatableObject
{
    public const string SectionName = "Encryption";
    public const int KeySizeBytes = 32;
    public const int MinBlindIndexKeySizeBytes = 32;

    /// <summary>Current AES-256 key (Base64, 32 bytes). Every new ciphertext uses it.</summary>
    [Required]
    public string MasterKey { get; init; } = string.Empty;

    /// <summary>HMAC-SHA256 pepper for blind indexes (Base64, at least 32 bytes). Rotating it
    /// invalidates every stored index, so it is not versioned — change it only with a full re-index.</summary>
    [Required]
    public string BlindIndexKey { get; init; } = string.Empty;

    /// <summary>Version stamped on new ciphertexts (<c>v{KeyVersion}:</c>).</summary>
    [Range(1, 9999)]
    public int KeyVersion { get; init; } = 1;

    /// <summary>Retired keys, by version, still needed to decrypt older rows during a rotation.
    /// Remove one only after the backfill has re-encrypted every row under the current key.</summary>
    public Dictionary<int, string> PreviousKeys { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!HasLength(MasterKey, KeySizeBytes, exact: true))
            yield return new ValidationResult(
                $"Encryption:MasterKey must be Base64 encoding exactly {KeySizeBytes} bytes.",
                [nameof(MasterKey)]);

        if (!HasLength(BlindIndexKey, MinBlindIndexKeySizeBytes, exact: false))
            yield return new ValidationResult(
                $"Encryption:BlindIndexKey must be Base64 encoding at least {MinBlindIndexKeySizeBytes} bytes.",
                [nameof(BlindIndexKey)]);

        if (PreviousKeys.ContainsKey(KeyVersion))
            yield return new ValidationResult(
                "Encryption:PreviousKeys must not contain the current KeyVersion.",
                [nameof(PreviousKeys)]);

        foreach (var (version, key) in PreviousKeys)
        {
            if (version < 1 || !HasLength(key, KeySizeBytes, exact: true))
                yield return new ValidationResult(
                    $"Encryption:PreviousKeys:{version} must be a positive version with a Base64 {KeySizeBytes}-byte key.",
                    [nameof(PreviousKeys)]);
        }
    }

    internal static bool TryDecodeKey(string? base64, out byte[] key)
    {
        key = [];
        if (string.IsNullOrWhiteSpace(base64))
            return false;

        try
        {
            key = Convert.FromBase64String(base64.Trim());
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool HasLength(string? base64, int length, bool exact) =>
        TryDecodeKey(base64, out var key) && (exact ? key.Length == length : key.Length >= length);
}
