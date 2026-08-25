using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class ProductSize : Entity
{
    public Guid ProductId { get; private set; }
    public Guid TenantId { get; private set; }
    public SizeValue Size { get; private set; }

    private ProductSize() : base(Guid.NewGuid()) { }

    internal ProductSize(Guid productId, Guid tenantId, SizeValue size)
        : base(Guid.NewGuid())
    {
        ProductId = productId;
        TenantId = tenantId;
        Size = size;
    }
}
