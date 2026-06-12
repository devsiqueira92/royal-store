namespace RoyalStore.Application.Abstractions.Services;

public interface ITenantContext
{
    Guid TenantId { get; }
}
