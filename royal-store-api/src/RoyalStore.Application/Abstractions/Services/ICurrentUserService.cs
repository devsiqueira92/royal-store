namespace RoyalStore.Application.Abstractions.Services;

public interface ICurrentUserService
{
    string? CurrentUserId { get; }
}
