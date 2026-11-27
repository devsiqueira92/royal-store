using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Profiles;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class ProfileRepository : IProfileRepository
{
    private const string CpfHash = nameof(Profile.Cpf) + "Hash";
    private const string EmailHash = nameof(Profile.Email) + "Hash";

    private readonly RoyalStoreContext _context;

    public ProfileRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Profile profile, CancellationToken ct = default) =>
        await _context.Profiles.AddAsync(profile, ct);

    public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Profiles.FirstOrDefaultAsync(p => p.Id == id, ct);

    // UserId is a globally unique FK to AspNetUsers.Id, so ignoring the tenant
    // query filter is safe here — it lets anonymous flows (login) and flows with
    // no tenant context resolve a profile by its account id.
    public async Task<Profile?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await _context.Profiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task<IReadOnlyDictionary<string, Profile>> GetByUserIdsAsync(
        IEnumerable<string> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToArray();
        if (ids.Length == 0)
            return new Dictionary<string, Profile>();

        var profiles = await _context.Profiles
            .Where(p => ids.Contains(p.UserId))
            .ToListAsync(ct);

        return profiles.ToDictionary(p => p.UserId);
    }

    // CPF and email are encrypted with a random nonce, so they're matched on their blind indexes.
    public async Task<bool> ExistsByCpfAsync(string cpf, Guid? excludeId, CancellationToken ct = default)
    {
        var hash = _context.ComputeBlindIndex<Profile>(nameof(Profile.Cpf), cpf);
        return await _context.Profiles.AnyAsync(
            p => EF.Property<string?>(p, CpfHash) == hash && (excludeId == null || p.Id != excludeId),
            ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken ct = default)
    {
        var hash = _context.ComputeBlindIndex<Profile>(nameof(Profile.Email), email);
        return await _context.Profiles.AnyAsync(
            p => EF.Property<string?>(p, EmailHash) == hash && (excludeId == null || p.Id != excludeId),
            ct);
    }

    public Task UpdateAsync(Profile profile, CancellationToken ct = default)
    {
        _context.Profiles.Update(profile);
        return Task.CompletedTask;
    }
}
