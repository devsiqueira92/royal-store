namespace RoyalStore.Application.Abstractions.Security;

/// <summary>
/// Application-level encryption for PII and secrets kept at rest. Values are encrypted with a
/// random nonce, so the same plaintext never produces the same ciphertext — use
/// <see cref="ComputeBlindIndex(string, BlindIndexKind)"/> for any exact-match lookup or
/// uniqueness rule on an encrypted field.
/// <para>Never use this for passwords: those are hashed by ASP.NET Identity's password hasher.</para>
/// </summary>
public interface IEncryptionService
{
    /// <summary>Returns a versioned, authenticated ciphertext (<c>v{version}:{base64}</c>).</summary>
    string Encrypt(string plainText);

    /// <summary>Reverses <see cref="Encrypt"/> using the key named by the payload's version.</summary>
    /// <exception cref="System.Security.Cryptography.CryptographicException">The payload is
    /// malformed, names an unknown key version, or fails authentication (tampered).</exception>
    string Decrypt(string cipherText);

    string? EncryptNullable(string? plainText);
    string? DecryptNullable(string? cipherText);

    /// <summary>Deterministic keyed hash (HMAC-SHA256) of the trimmed value.</summary>
    string ComputeBlindIndex(string plainText);

    /// <summary>Deterministic keyed hash (HMAC-SHA256) of the value normalized per
    /// <paramref name="kind"/>, so differently-formatted inputs of the same value match.</summary>
    string ComputeBlindIndex(string plainText, BlindIndexKind kind);
}
