using RoyalStore.Domain.Common;
using RoyalStore.Domain.Identity;

namespace RoyalStore.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<UserEntity?> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(UserEntity user, string password);
    Task<IList<string>> GetRolesAsync(UserEntity user);
    Task<UserEntity?> FindByIdAsync(string userId);
    Task<Result<UserEntity>> CreateAsync(UserEntity user, string password);
    Task<Result> AddToRoleAsync(UserEntity user, string role);
    Task<string> GenerateEmailConfirmationTokenAsync(UserEntity user);
    Task<Result> ConfirmEmailAsync(UserEntity user, string token);
    Task<UserEntity?> FindByEmailAndTenantAsync(string email, Guid tenantId);
    Task<string> GeneratePasswordResetTokenAsync(UserEntity user);
    Task<Result> ResetPasswordAsync(UserEntity user, string token, string newPassword);
    Task<Result> UpdateSecurityStampAsync(UserEntity user);
    Task<bool> UserNameExistsInTenantAsync(string userName, Guid tenantId, CancellationToken ct = default);
}
