namespace RoyalStore.Application.DTOs;

public sealed record TokenResponse(string Token, DateTimeOffset ExpiresAt);
