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

        // Sum of the lines' amounts (064 follow-up); kept by the create and update handlers.
        builder.Property(e => e.TotalValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(450).IsRequired();

        builder.HasIndex(e => e.TenantId);

        // Reusable: the foreign key is on the product, so many products can name the same list.
        // Deleting a list leaves its products without one; deleting a product never touches the list.
        builder.HasMany<Product>()
            .WithOne(p => p.CostComposition)
            .HasForeignKey(p => p.CostCompositionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Items)
            .WithOne()
            .HasForeignKey(i => i.CostCompositionListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
