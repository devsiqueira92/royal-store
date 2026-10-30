using RoyalStore.Domain.Profiles;
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
    public static SellerResponse From(Seller seller, Profile? profile) =>
        new(seller.Id, seller.TenantId,
            profile?.FullName ?? string.Empty, profile?.Phone, profile?.Cpf, profile?.Email,
            seller.IsActive, seller.UserId, profile?.CompletedAt);
}
