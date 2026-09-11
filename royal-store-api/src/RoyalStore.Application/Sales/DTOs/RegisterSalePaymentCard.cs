namespace RoyalStore.Application.Sales.DTOs;

/// <summary>Card detail for a card payment line. Present only for card-based payment methods.</summary>
public record RegisterSalePaymentCard(string? Flag, string? Type, int? Installments);
