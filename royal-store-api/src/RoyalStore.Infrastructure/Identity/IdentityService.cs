using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Identity;

namespace RoyalStore.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<UserEntity> _userManager;

    public IdentityService(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    public Task<UserEntity?> FindByEmailAsync(string email) =>
        _userManager.FindByEmailAsync(email);

    public Task<UserEntity?> FindByIdAsync(string userId) =>
        _userManager.FindByIdAsync(userId);

    public Task<UserEntity?> FindByNameAsync(string userName) =>
        _userManager.FindByNameAsync(userName);

    public async Task<bool> CheckPasswordAsync(UserEntity user, string password) =>
        await _userManager.CheckPasswordAsync(user, password);

    public Task<IList<string>> GetRolesAsync(UserEntity user) =>
        _userManager.GetRolesAsync(user);

    public async Task<Result<UserEntity>> CreateAsync(UserEntity user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Error.Validation("registration.invalid_password", message);
        }
        return user;
    }

    public async Task<Result> ChangePasswordAsync(UserEntity user, string currentPassword, string newPassword)
    {
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("seller.password_change_failed", message));
        }

        return Result.Success();
    }

    public async Task<Result> ClearPasswordChangeFlagAsync(UserEntity user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Unexpected("auth.user_update_failed", message));
        }

        return Result.Success();
    }

    public async Task<Result> AddToRoleAsync(UserEntity user, string role)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("registration.role_assignment_failed", message));
        }
        return Result.Success();
    }

    public Task<string> GenerateEmailConfirmationTokenAsync(UserEntity user) =>
        _userManager.GenerateEmailConfirmationTokenAsync(user);

    public async Task<Result> ConfirmEmailAsync(UserEntity user, string token)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return Result.Failure(Error.Validation("verification.invalid_token",
                "The verification link is invalid or has expired. Please request a new verification email."));

        return Result.Success();
    }

    public Task<UserEntity?> FindByEmailAndTenantAsync(string email, Guid tenantId) =>
        _userManager.Users
            .FirstOrDefaultAsync(u =>
                u.NormalizedEmail == _userManager.NormalizeEmail(email) &&
                u.TenantId == tenantId);

    public Task<string> GeneratePasswordResetTokenAsync(UserEntity user) =>
        _userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<Result> ResetPasswordAsync(UserEntity user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("auth.invalid_reset_token", message));
        }

        return Result.Success();
    }

    public async Task<Result> UpdateSecurityStampAsync(UserEntity user)
    {
        var result = await _userManager.UpdateSecurityStampAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Unexpected("auth.security_stamp_update_failed", message));
        }

        return Result.Success();
    }

    public Task<bool> UserNameExistsInTenantAsync(string userName, Guid tenantId, CancellationToken ct = default) =>
        _userManager.Users.AnyAsync(
            u => u.NormalizedUserName == _userManager.NormalizeName(userName) && u.TenantId == tenantId, ct);

    public async Task<Result> ChangeEmailAsync(UserEntity user, string newEmail)
    {
        // SetEmailAsync already resets EmailConfirmed to false and rotates SecurityStamp
        // internally, invalidating any previously issued confirmation token in one call.
        var result = await _userManager.SetEmailAsync(user, newEmail);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("email_change.invalid_email", message));
        }

        return Result.Success();
    }
}
