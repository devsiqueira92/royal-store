using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Application.Options;

public sealed class ResendConfirmationOptions
{
    [Range(1, 3600)]
    public int CooldownSeconds { get; init; } = 60;

    [Range(1, 100)]
    public int MaxAttemptsPerWindow { get; init; } = 5;

    [Range(1, 168)]
    public int WindowHours { get; init; } = 24;

    [Range(1, 1000)]
    public int IpPermitLimit { get; init; } = 20;

    [Range(1, 1440)]
    public int IpWindowMinutes { get; init; } = 1;
}
