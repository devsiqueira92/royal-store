using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class ProductTag : Entity
{
    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid TenantId { get; private set; }
    public Category Category { get; private set; } = null!;

    private ProductTag() : base(Guid.NewGuid()) { }

    internal ProductTag(Guid productId, Guid tenantId, Guid categoryId)
        : base(Guid.NewGuid())
    {
        ProductId = productId;
        TenantId = tenantId;
        CategoryId = categoryId;
    }
}
