using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class SaleItem : Entity
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public string Ncm { get; private set; } = string.Empty;
    public string? Cest { get; private set; }

    private SaleItem() : base(Guid.NewGuid()) { }

    private SaleItem(Guid id, Guid saleId, Guid productId, decimal quantity,
        decimal unitPrice, decimal totalPrice, string ncm, string? cest)
        : base(id)
    {
        SaleId = saleId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
        Ncm = ncm;
        Cest = cest;
    }

    public static Result<SaleItem> Create(Guid id, Guid saleId, Guid productId,
        decimal quantity, decimal unitPrice, decimal totalPrice, string ncm, string? cest)
    {
        if (id == Guid.Empty)
            return Error.Validation("saleitem.id_required", "Sale item ID is required.");

        if (saleId == Guid.Empty)
            return Error.Validation("saleitem.sale_required", "Sale identifier is required.");

        if (productId == Guid.Empty)
            return Error.Validation("saleitem.product_required", "Product identifier is required.");

        if (quantity <= 0)
            return Error.Validation("saleitem.quantity_invalid", "Quantity must be greater than zero.");

        if (unitPrice < 0)
            return Error.Validation("saleitem.unit_price_negative", "Unit price cannot be negative.");

        if (totalPrice < 0)
            return Error.Validation("saleitem.total_price_negative", "Total price cannot be negative.");

        if (string.IsNullOrWhiteSpace(ncm))
            return Error.Validation("saleitem.ncm_required", "NCM is required.");

        ncm = ncm.Trim();
        if (ncm.Length > 8)
            return Error.Validation("saleitem.ncm_too_long", "NCM must not exceed 8 characters.");

        if (cest is not null)
        {
            cest = cest.Trim();
            if (cest.Length > 7)
                return Error.Validation("saleitem.cest_too_long", "CEST must not exceed 7 characters.");
        }

        return new SaleItem(id, saleId, productId, quantity, unitPrice, totalPrice, ncm, cest);
    }
}
