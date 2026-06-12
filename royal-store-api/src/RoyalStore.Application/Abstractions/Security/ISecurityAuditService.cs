namespace RoyalStore.Application.Abstractions.Security;

public interface ISecurityAuditService
{
    Task RecordAsync(
        string eventType,
        Guid tenantId,
        string failureReason,
        string obfuscatedIdentifier,
        CancellationToken cancellationToken = default);
}
