using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Infrastructure.Security.Encryption;

namespace RoyalStore.Infrastructure.Tests.Security;

public sealed class AesGcmEncryptionServiceTests
{
    private readonly AesGcmEncryptionService _sut = TestKeys.Service();

    [Theory]
    [InlineData("12345678909")]
    [InlineData("")]
    [InlineData("Rua São João, 123 — apto 45 🏠")]
    public void Encrypt_then_decrypt_returns_the_original(string plain)
    {
        var cipher = _sut.Encrypt(plain);

        Assert.StartsWith("v1:", cipher);
        if (plain.Length > 0)
            Assert.DoesNotContain(plain, cipher, StringComparison.Ordinal);
        Assert.Equal(plain, _sut.Decrypt(cipher));
    }

    [Fact]
    public void Encrypt_uses_a_fresh_nonce_per_call()
    {
        var ciphers = Enumerable.Range(0, 200).Select(_ => _sut.Encrypt("same value")).ToList();

        Assert.Equal(ciphers.Count, ciphers.Distinct().Count());

        var nonces = ciphers
            .Select(c => Convert.FromBase64String(c[3..])[..EncryptedPayload.NonceSize])
            .Select(Convert.ToHexString)
            .ToList();
        Assert.Equal(nonces.Count, nonces.Distinct().Count());
    }

    [Theory]
    [InlineData(0)]                              // nonce
    [InlineData(EncryptedPayload.NonceSize)]     // tag
    [InlineData(EncryptedPayload.HeaderSize)]    // ciphertext
    public void Decrypt_rejects_a_tampered_payload(int byteToFlip)
    {
        var cipher = _sut.Encrypt("12345678909");
        var payload = Convert.FromBase64String(cipher[3..]);
        payload[byteToFlip] ^= 0x01;

        Assert.ThrowsAny<CryptographicException>(() => _sut.Decrypt("v1:" + Convert.ToBase64String(payload)));
    }

    [Fact]
    public void Decrypt_rejects_a_payload_relabelled_to_another_key_version()
    {
        // Same key under both versions: only the authenticated prefix (AAD) can catch the swap.
        var sut = TestKeys.Service(TestKeys.Options(version: 2, master: TestKeys.MasterV1,
            previous: new() { [1] = TestKeys.MasterV1 }));
        var cipher = sut.Encrypt("secret");

        Assert.ThrowsAny<CryptographicException>(() => sut.Decrypt("v1:" + cipher[3..]));
    }

    [Theory]
    [InlineData("12345678909")]                  // legacy plaintext
    [InlineData("v1:not-base64!")]
    [InlineData("v1:AAAA")]                      // truncated
    [InlineData("v9:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")] // unknown version
    [InlineData("vx:AAAA")]
    public void Decrypt_rejects_malformed_input(string input) =>
        Assert.ThrowsAny<CryptographicException>(() => _sut.Decrypt(input));

    [Fact]
    public void Rotation_reads_old_ciphertexts_and_writes_with_the_new_key()
    {
        var old = _sut.Encrypt("12345678909");
        var rotated = TestKeys.RotatedService();

        Assert.Equal("12345678909", rotated.Decrypt(old));
        Assert.StartsWith("v2:", rotated.Encrypt("x"));
        Assert.ThrowsAny<CryptographicException>(() => _sut.Decrypt(rotated.Encrypt("x")));
    }

    [Fact]
    public void Nullable_helpers_pass_null_through()
    {
        Assert.Null(_sut.EncryptNullable(null));
        Assert.Null(_sut.DecryptNullable(null));
        Assert.Equal("a", _sut.DecryptNullable(_sut.EncryptNullable("a")));
    }

    [Fact]
    public void Blind_index_is_deterministic_hex_sha256()
    {
        var a = _sut.ComputeBlindIndex("12345678909", BlindIndexKind.Digits);

        Assert.Equal(a, TestKeys.Service().ComputeBlindIndex("12345678909", BlindIndexKind.Digits));
        Assert.Equal(64, a.Length);
        Assert.Matches("^[0-9a-f]{64}$", a);
    }

    [Theory]
    [InlineData(BlindIndexKind.Digits, "123.456.789-09", "12345678909")]
    [InlineData(BlindIndexKind.Digits, "(41) 99999-0000", "41999990000")]
    [InlineData(BlindIndexKind.Email, "  John.Doe@Example.COM ", "john.doe@example.com")]
    [InlineData(BlindIndexKind.Text, "  value ", "value")]
    public void Blind_index_normalizes_equivalent_inputs(BlindIndexKind kind, string formatted, string canonical) =>
        Assert.Equal(_sut.ComputeBlindIndex(canonical, kind), _sut.ComputeBlindIndex(formatted, kind));

    [Fact]
    public void Blind_index_distinguishes_values_and_keys()
    {
        var other = TestKeys.Service(TestKeys.Options(blindIndex: TestKeys.NewKey()));

        Assert.NotEqual(_sut.ComputeBlindIndex("a@b.com", BlindIndexKind.Email),
            _sut.ComputeBlindIndex("a@c.com", BlindIndexKind.Email));
        Assert.NotEqual(_sut.ComputeBlindIndex("a@b.com", BlindIndexKind.Email),
            other.ComputeBlindIndex("a@b.com", BlindIndexKind.Email));
        Assert.NotEqual(_sut.ComputeBlindIndex("Text"), _sut.ComputeBlindIndex("text"));
    }

    public static TheoryData<EncryptionOptions> InvalidOptions => new()
    {
        TestKeys.Options(master: TestKeys.NewKey(16)),
        TestKeys.Options(master: TestKeys.NewKey(64)),
        TestKeys.Options(master: "not base64!"),
        TestKeys.Options(master: ""),
        TestKeys.Options(blindIndex: TestKeys.NewKey(16)),
        TestKeys.Options(version: 0),
        TestKeys.Options(previous: new() { [1] = TestKeys.MasterV2 }),
        TestKeys.Options(version: 2, previous: new() { [1] = TestKeys.NewKey(16) }),
    };

    [Theory]
    [MemberData(nameof(InvalidOptions))]
    public void Invalid_configuration_is_rejected_by_validation_and_the_service(EncryptionOptions options)
    {
        Assert.False(Validator.TryValidateObject(options, new ValidationContext(options), [], validateAllProperties: true));
        Assert.Throws<ArgumentException>(() => TestKeys.Service(options));
    }

    [Fact]
    public void Valid_configuration_passes_validation()
    {
        var options = TestKeys.Options(version: 2, master: TestKeys.MasterV2, previous: new() { [1] = TestKeys.MasterV1 });

        Assert.True(Validator.TryValidateObject(options, new ValidationContext(options), [], validateAllProperties: true));
    }
}
