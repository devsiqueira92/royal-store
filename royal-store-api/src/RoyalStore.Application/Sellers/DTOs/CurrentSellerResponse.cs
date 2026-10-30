using RoyalStore.Domain.Profiles;

namespace RoyalStore.Application.Sellers.DTOs;

public record CurrentSellerResponse(
    string FullName,
    string? Phone,
    string? Email,
    bool IsActive,
    DateTimeOffset? CreatedAt,
    bool IsProfileComplete)
{
    public static CurrentSellerResponse From(Profile profile) =>
        new(profile.FullName, profile.Phone, profile.Email, true,
            profile.CreatedAt, profile.IsComplete);
}
