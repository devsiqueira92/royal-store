using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Abstractions.Data;

public sealed record SaleWithSellerName(Sale Sale, string? SellerName);
