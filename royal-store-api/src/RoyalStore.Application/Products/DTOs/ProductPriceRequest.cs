namespace RoyalStore.Application.Products.DTOs;

/// <summary>
/// One price as a product/service request sends it. <c>SalePrice</c> is nullable on the wire so an
/// empty price can be refused with a message naming its list, instead of a deserialization error.
/// </summary>
public record ProductPriceRequest(Guid PriceListId, decimal? SalePrice, decimal? MinPrice);
