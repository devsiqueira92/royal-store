using RoyalStore.Domain.Sellers;

namespace RoyalStore.Application.Sellers.DTOs;

public record SellerResponse(
    Guid Id,
    Guid TenantId,
    string FullName,
    string? Phone,
    string? Cpf,
    string? Email,
    bool IsActive,
    string? UserId,
    DateTimeOffset? OnboardingCompletedAt,
    string? Role = null,
    IReadOnlyList<string>? Permissions = null)
{
    public static SellerResponse From(Seller seller) =>
        new(seller.Id, seller.TenantId, seller.FullName, seller.Phone, seller.Cpf, seller.Email,
            seller.IsActive, seller.UserId, seller.OnboardingCompletedAt);
}
