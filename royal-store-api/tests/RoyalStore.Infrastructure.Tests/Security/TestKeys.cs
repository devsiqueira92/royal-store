using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using RoyalStore.Infrastructure.Security.Encryption;

namespace RoyalStore.Infrastructure.Tests.Security;

internal static class TestKeys
{
    public static readonly string MasterV1 = NewKey();
    public static readonly string MasterV2 = NewKey();
    public static readonly string BlindIndex = NewKey();

    public static string NewKey(int bytes = 32) => Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes));

    public static EncryptionOptions Options(
        string? master = null, string? blindIndex = null, int version = 1,
        Dictionary<int, string>? previous = null) => new()
    {
        MasterKey = master ?? MasterV1,
        BlindIndexKey = blindIndex ?? BlindIndex,
        KeyVersion = version,
        PreviousKeys = previous ?? [],
    };

    public static AesGcmEncryptionService Service(EncryptionOptions? options = null) =>
        new(Microsoft.Extensions.Options.Options.Create(options ?? Options()));

    /// <summary>Current key v2, v1 kept for decryption — mid-rotation.</summary>
    public static AesGcmEncryptionService RotatedService() =>
        Service(Options(master: MasterV2, version: 2, previous: new() { [1] = MasterV1 }));
}
