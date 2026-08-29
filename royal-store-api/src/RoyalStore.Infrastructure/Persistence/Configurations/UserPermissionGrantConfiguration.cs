using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class UserPermissionGrantConfiguration : IEntityTypeConfiguration<UserPermissionGrant>
{
    public void Configure(EntityTypeBuilder<UserPermissionGrant> builder)
    {
        builder.ToTable("UserPermissionGrants");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.TenantId).IsRequired();

        builder.Property(g => g.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(g => g.Permission)
            .HasConversion(
                p => PermissionCatalog.NameOf(p),
                s => Parse(s))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(g => g.CreatedAt).IsRequired();
        builder.Property(g => g.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(g => g.UpdatedAt).IsRequired();
        builder.Property(g => g.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => new { g.TenantId, g.UserId, g.Permission }).IsUnique();
        builder.HasIndex(g => g.TenantId);
    }

    private static Permission Parse(string name) =>
        PermissionCatalog.TryParse(name, out var permission)
            ? permission
            : throw new InvalidOperationException($"Unknown permission '{name}' in UserPermissionGrants.");
}
