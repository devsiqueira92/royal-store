using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Profiles;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired();

        builder.Property(p => p.UserId).HasMaxLength(450).IsRequired();

        builder.Property(p => p.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.Phone)
            .HasMaxLength(20);

        builder.Property(p => p.Cpf)
            .HasMaxLength(11);

        builder.Property(p => p.Email)
            .HasMaxLength(256);

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

        builder.HasIndex(p => new { p.TenantId, p.Cpf })
            .IsUnique()
            .HasFilter("[Cpf] IS NOT NULL");

        builder.HasIndex(p => new { p.TenantId, p.Email })
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.HasIndex(p => p.TenantId);
    }
}
