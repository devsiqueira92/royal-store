using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Identity;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetRequestConfiguration : IEntityTypeConfiguration<PasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<PasswordResetRequest> builder)
    {
        builder.ToTable("PasswordResetRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.Property(r => r.IsConsumed)
            .IsRequired();

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();
        builder.Property(r => r.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(r => new { r.UserId, r.TenantId, r.CreatedAt });
    }
}
