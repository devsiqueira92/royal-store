using MediatR;

namespace RoyalStore.Application.Tenants.Events;

public sealed record TenantRegisteredEvent(Guid TenantId, string UserId, string Email) : INotification;
