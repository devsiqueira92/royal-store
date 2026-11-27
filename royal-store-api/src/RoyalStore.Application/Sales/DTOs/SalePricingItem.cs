namespace RoyalStore.Application.Sales.DTOs;

/// <summary>One cart line to price: which variant and how many.</summary>
public record SalePricingItem(Guid VariantId, decimal Quantity);
