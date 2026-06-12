using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Tenants;

public sealed record TenantRegisteredDomainEvent(Guid TenantId) : IDomainEvent;
