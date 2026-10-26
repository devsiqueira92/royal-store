using RoyalStore.Domain.Sellers;

namespace RoyalStore.Application.Sellers.DTOs;

public record CurrentSellerResponse(
    string FullName,
    string? Phone,
    string? Email,
    bool IsActive,
    DateTimeOffset CreatedAt,
    string TenantName)
{
    public static CurrentSellerResponse From(Seller seller, string? email, string tenantName) =>
        new(seller.FullName, seller.Phone, email, seller.IsActive, seller.CreatedAt, tenantName);
}
