using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Domain.Security;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Security;

public sealed class SecurityAuditService(RoyalStoreContext context) : ISecurityAuditService
{
    public async Task RecordAsync(
        string eventType,
        Guid tenantId,
        string failureReason,
        string obfuscatedIdentifier,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = SecurityAuditEvent.Create(eventType, tenantId, failureReason, obfuscatedIdentifier);
        await context.SecurityAuditEvents.AddAsync(auditEvent, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
