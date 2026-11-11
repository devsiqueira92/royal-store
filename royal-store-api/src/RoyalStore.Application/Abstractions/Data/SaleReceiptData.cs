using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Abstractions.Data;

public sealed record SaleReceiptData(
    Sale Sale, string? StoreName,
    IReadOnlyList<ReceiptItemResponse> Items, IReadOnlyList<ReceiptPaymentResponse> Payments);
