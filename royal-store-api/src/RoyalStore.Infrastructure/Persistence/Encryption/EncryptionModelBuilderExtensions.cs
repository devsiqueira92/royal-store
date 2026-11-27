using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoyalStore.Application.Abstractions.Security;

namespace RoyalStore.Infrastructure.Persistence.Encryption;

/// <summary>
/// Declarative encryption for EF mappings. Configurations mark properties with
/// <see cref="IsEncrypted{TProperty}"/> / <see cref="HasBlindIndex{TEntity}"/> (they have no access to
/// services); <see cref="ApplyEncryption"/> then attaches the converters once the model is built.
/// Entities keep plaintext in memory — encryption happens only at the database boundary.
/// </summary>
public static class EncryptionModelBuilderExtensions
{
    internal const string EncryptedAnnotation = "RoyalStore:Encrypted";
    internal const string BlindIndexSourceAnnotation = "RoyalStore:BlindIndexSource";
    internal const string BlindIndexKindAnnotation = "RoyalStore:BlindIndexKind";

    /// <summary>Length of a hex HMAC-SHA256 blind index.</summary>
    public const int BlindIndexLength = 64;

    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>Stores the property AES-GCM encrypted (<c>text</c> column). Supports
    /// <see cref="string"/> and <see cref="DateOnly"/>. An encrypted column can't be filtered,
    /// sorted or indexed — pair it with <see cref="HasBlindIndex{TEntity}"/> for exact-match lookups.</summary>
    public static PropertyBuilder<TProperty> IsEncrypted<TProperty>(this PropertyBuilder<TProperty> builder) =>
        builder.HasAnnotation(EncryptedAnnotation, true);

    /// <summary>Adds the shadow column <c>{source}Hash</c> holding the blind index of
    /// <paramref name="sourceProperty"/>, kept in sync by <see cref="BlindIndexInterceptor"/>.
    /// Query it with <c>EF.Property&lt;string?&gt;(e, BlindIndex.PropertyName(...))</c>.</summary>
    public static PropertyBuilder<string?> HasBlindIndex<TEntity>(
        this EntityTypeBuilder<TEntity> builder, string sourceProperty, BlindIndexKind kind)
        where TEntity : class =>
        builder.Property<string?>(BlindIndex.PropertyName(sourceProperty))
            .HasMaxLength(BlindIndexLength)
            .HasAnnotation(BlindIndexSourceAnnotation, sourceProperty)
            .HasAnnotation(BlindIndexKindAnnotation, kind);

    /// <summary>Attaches encrypting converters to every property marked <see cref="IsEncrypted{TProperty}"/>.
    /// Call after all configurations are applied.</summary>
    public static ModelBuilder ApplyEncryption(this ModelBuilder modelBuilder, IEncryptionService encryption)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.FindAnnotation(EncryptedAnnotation)?.Value is not true)
                    continue;

                property.SetValueConverter(CreateConverter(property.ClrType, encryption)
                    ?? throw new InvalidOperationException(
                        $"{entityType.DisplayName()}.{property.Name}: encryption supports string and DateOnly, not {property.ClrType.Name}."));

                // Ciphertext is longer than the plaintext and its length isn't fixed: the domain
                // enforces plaintext limits, the column is unbounded text.
                property.SetMaxLength(null);
                property.SetColumnType(null);
            }
        }

        return modelBuilder;
    }

    private static ValueConverter? CreateConverter(Type clrType, IEncryptionService encryption)
    {
        var type = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (type == typeof(string))
            return new ValueConverter<string, string>(
                v => encryption.Encrypt(v),
                v => encryption.Decrypt(v));

        if (type == typeof(DateOnly))
            return new ValueConverter<DateOnly, string>(
                v => encryption.Encrypt(v.ToString(DateFormat, CultureInfo.InvariantCulture)),
                v => DateOnly.ParseExact(encryption.Decrypt(v), DateFormat, CultureInfo.InvariantCulture));

        return null;
    }
}
