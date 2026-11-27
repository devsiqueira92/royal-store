using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using RoyalStore.Application.Abstractions.Security;

namespace RoyalStore.Infrastructure.Security.Encryption;

/// <summary>
/// AES-256-GCM with a fresh random 96-bit nonce per call. The version prefix is bound to the
/// ciphertext as associated data, so a payload can't be relabelled to another key version.
/// Keys are decoded once; rotating them (new <c>KeyVersion</c> + old key under
/// <c>PreviousKeys</c>) takes effect on restart, and the startup backfill re-encrypts old rows.
/// </summary>
public sealed class AesGcmEncryptionService : IEncryptionService
{
    private readonly int _currentVersion;
    private readonly IReadOnlyDictionary<int, byte[]> _keys;
    private readonly byte[] _blindIndexKey;

    public AesGcmEncryptionService(IOptions<EncryptionOptions> options)
    {
        var value = options.Value;

        _currentVersion = value.KeyVersion;
        if (_currentVersion < 1)
            throw new ArgumentException("Encryption:KeyVersion must be at least 1.", nameof(options));

        var keys = new Dictionary<int, byte[]>
        {
            [_currentVersion] = DecodeKey(value.MasterKey, "Encryption:MasterKey", exact: true,
                EncryptionOptions.KeySizeBytes)
        };

        foreach (var (version, key) in value.PreviousKeys)
        {
            if (version < 1 || version == _currentVersion)
                throw new ArgumentException(
                    $"Encryption:PreviousKeys:{version} must be positive and differ from KeyVersion.", nameof(options));

            keys[version] = DecodeKey(key, $"Encryption:PreviousKeys:{version}", exact: true,
                EncryptionOptions.KeySizeBytes);
        }

        _keys = keys;
        _blindIndexKey = DecodeKey(value.BlindIndexKey, "Encryption:BlindIndexKey", exact: false,
            EncryptionOptions.MinBlindIndexKeySizeBytes);
    }

    public int CurrentKeyVersion => _currentVersion;

    public string Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        var prefix = EncryptedPayload.Prefix(_currentVersion);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var payload = new byte[EncryptedPayload.HeaderSize + plainBytes.Length];

        var nonce = payload.AsSpan(0, EncryptedPayload.NonceSize);
        var tag = payload.AsSpan(EncryptedPayload.NonceSize, EncryptedPayload.TagSize);
        var cipher = payload.AsSpan(EncryptedPayload.HeaderSize);

        RandomNumberGenerator.Fill(nonce);

        using (var aes = new AesGcm(_keys[_currentVersion], EncryptedPayload.TagSize))
            aes.Encrypt(nonce, plainBytes, cipher, tag, Encoding.ASCII.GetBytes(prefix));

        CryptographicOperations.ZeroMemory(plainBytes);
        return prefix + Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentNullException.ThrowIfNull(cipherText);

        if (!EncryptedPayload.TryParse(cipherText, out var version, out var body))
            throw new CryptographicException("Value is not an encrypted payload.");

        if (!_keys.TryGetValue(version, out var key))
            throw new CryptographicException($"No key configured for encryption version {version}.");

        byte[] payload;
        try
        {
            payload = Convert.FromBase64String(body);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("Encrypted payload is not valid Base64.", ex);
        }

        if (payload.Length < EncryptedPayload.HeaderSize)
            throw new CryptographicException("Encrypted payload is truncated.");

        var nonce = payload.AsSpan(0, EncryptedPayload.NonceSize);
        var tag = payload.AsSpan(EncryptedPayload.NonceSize, EncryptedPayload.TagSize);
        var cipher = payload.AsSpan(EncryptedPayload.HeaderSize);
        var plainBytes = new byte[cipher.Length];

        // Throws AuthenticationTagMismatchException (a CryptographicException) on any tampering.
        using (var aes = new AesGcm(key, EncryptedPayload.TagSize))
            aes.Decrypt(nonce, cipher, tag, plainBytes,
                Encoding.ASCII.GetBytes(EncryptedPayload.Prefix(version)));

        var plainText = Encoding.UTF8.GetString(plainBytes);
        CryptographicOperations.ZeroMemory(plainBytes);
        return plainText;
    }

    public string? EncryptNullable(string? plainText) => plainText is null ? null : Encrypt(plainText);

    public string? DecryptNullable(string? cipherText) => cipherText is null ? null : Decrypt(cipherText);

    public string ComputeBlindIndex(string plainText) => ComputeBlindIndex(plainText, BlindIndexKind.Text);

    public string ComputeBlindIndex(string plainText, BlindIndexKind kind)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        var normalized = Normalize(plainText, kind);
        var hash = HMACSHA256.HashData(_blindIndexKey, Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexStringLower(hash);
    }

    internal static string Normalize(string value, BlindIndexKind kind) => kind switch
    {
        BlindIndexKind.Text => value.Trim().Normalize(NormalizationForm.FormC),
        BlindIndexKind.Email => value.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant(),
        BlindIndexKind.Digits => string.Concat(value.Where(char.IsAsciiDigit)),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown blind index kind."),
    };

    private static byte[] DecodeKey(string? base64, string name, bool exact, int size)
    {
        if (!EncryptionOptions.TryDecodeKey(base64, out var key) ||
            (exact ? key.Length != size : key.Length < size))
            throw new ArgumentException(
                exact
                    ? $"{name} must be Base64 encoding exactly {size} bytes."
                    : $"{name} must be Base64 encoding at least {size} bytes.");

        return key;
    }
}
