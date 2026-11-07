using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Identity;

namespace RoyalStore.Application.Abstractions.Authentication;

public interface ITokenService
{
    TokenResponse GenerateToken(
        UserEntity user,
        string role,
        bool onboardingCompleted,
        bool isProfileComplete,
        IReadOnlyCollection<string> permissions);
}
