using RoyalStore.Domain.Sellers;

namespace RoyalStore.Application.Sellers.DTOs;

public record SellerResponse(
    Guid Id,
    Guid TenantId,
    string FullName,
    string Phone,
    string Cpf,
    bool IsActive,
    string? UserId)
{
    public static SellerResponse From(Seller seller) =>
        new(seller.Id, seller.TenantId, seller.FullName, seller.Phone, seller.Cpf, seller.IsActive, seller.UserId);
}
