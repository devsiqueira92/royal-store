using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Security;

public sealed class SecurityAuditEvent : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string FailureReason { get; private set; } = string.Empty;
    public string ObfuscatedIdentifier { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private SecurityAuditEvent() { }

    private SecurityAuditEvent(
        string eventType,
        Guid tenantId,
        string failureReason,
        string obfuscatedIdentifier)
    {
        Id = Guid.NewGuid();
        EventType = eventType;
        TenantId = tenantId;
        OccurredAt = DateTimeOffset.UtcNow;
        FailureReason = failureReason;
        ObfuscatedIdentifier = obfuscatedIdentifier;
    }

    public static SecurityAuditEvent Create(
        string eventType,
        Guid tenantId,
        string failureReason,
        string obfuscatedIdentifier)
        => new(eventType, tenantId, failureReason, obfuscatedIdentifier);
}
