using RoyalStore.Application.Loyalty;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Customers.DTOs;

public record CashbackEntryResponse(
    Guid Id,
    decimal GrantedAmount,
    decimal RemainingAmount,
    Guid SaleId,
    DateTime EarnedAt,
    DateTime ExpiresAt,
    string State)
{
    public static CashbackEntryResponse From(CashbackEntry entry, DateTime now) =>
        new(
            entry.Id,
            entry.GrantedAmount,
            entry.RemainingAmount,
            entry.SaleId,
            entry.EarnedAt,
            entry.ExpiresAt,
            CashbackEntryStateResolver.Resolve(entry, now).ToString().ToLowerInvariant());
}
