using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Configurations;

public sealed class CostCompositionListConfiguration : IEntityTypeConfiguration<CostCompositionList>
{
    public void Configure(EntityTypeBuilder<CostCompositionList> builder)
    {
        builder.ToTable("CostCompositionLists");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(CostCompositionList.NameMaxLength)
            .IsRequired();

        // Moves on link and unlink only. Two products naming the same list at once change the same
        // row, which the unique index below cannot catch; this makes the second UPDATE affect zero
        // rows and fail instead of taking the list over (064 research D4).
        builder.Property(e => e.Version)
            .IsRequired()
            .IsConcurrencyToken();

        // Sum of the lines' amounts (064 follow-up); kept by the create and update handlers.
        builder.Property(e => e.TotalValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        // A product has at most one list. PostgreSQL treats NULLs as distinct, so any number of
        // lists linked to no product coexist under this index (064 FR-002).
        builder.HasIndex(e => e.ProductId).IsUnique();
        builder.HasIndex(e => e.TenantId);

        // Optional: a list exists before its product does. Removing a product removes its list and,
        // through the next relationship, the list's lines (064 edge case "physically removed").
        builder.HasOne<Product>()
            .WithOne(p => p.CostComposition)
            .HasForeignKey<CostCompositionList>(e => e.ProductId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Items)
            .WithOne()
            .HasForeignKey(i => i.CostCompositionListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
