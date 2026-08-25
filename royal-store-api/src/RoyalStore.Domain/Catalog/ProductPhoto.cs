using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class ProductPhoto : Entity, IAuditableEntity
{
    public Guid ProductId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public bool IsCover { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private ProductPhoto() : base(Guid.NewGuid()) { }

    internal ProductPhoto(Guid productId, Guid tenantId, string url)
        : base(Guid.NewGuid())
    {
        ProductId = productId;
        TenantId = tenantId;
        Url = url;
        IsCover = false;
    }

    public void SetAsCover() => IsCover = true;
    public void UnsetCover() => IsCover = false;
}
