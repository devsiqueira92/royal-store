using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Profiles;
using RoyalStore.Infrastructure.Persistence.Encryption;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    private static readonly string CpfHash = BlindIndex.PropertyName(nameof(Profile.Cpf));
    private static readonly string EmailHash = BlindIndex.PropertyName(nameof(Profile.Email));

    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired();

        builder.Property(p => p.UserId).HasMaxLength(450).IsRequired();

        builder.Property(p => p.FullName)
            .HasMaxLength(150)
            .IsRequired();

        // PII, encrypted at rest. CPF and email are unique per tenant, enforced on their blind
        // indexes (the ciphertext is randomized, so it can't be compared).
        builder.Property(p => p.Phone).IsEncrypted();

        builder.Property(p => p.Cpf).IsEncrypted();
        builder.HasBlindIndex(nameof(Profile.Cpf), BlindIndexKind.Digits);

        builder.Property(p => p.Email).IsEncrypted();
        builder.HasBlindIndex(nameof(Profile.Email), BlindIndexKind.Email);

        builder.Property(p => p.CompletedAt);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
        builder.Property(p => p.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasOne<UserEntity>()
            .WithOne()
            .HasForeignKey<Profile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId).IsUnique();

        builder.HasIndex(nameof(Profile.TenantId), CpfHash)
            .IsUnique()
            .HasFilter($"\"{CpfHash}\" IS NOT NULL");

        builder.HasIndex(nameof(Profile.TenantId), EmailHash)
            .IsUnique()
            .HasFilter($"\"{EmailHash}\" IS NOT NULL");

        builder.HasIndex(p => p.TenantId);
    }
}
