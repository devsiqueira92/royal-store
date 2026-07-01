using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Application.Options;

public sealed class ChangePendingEmailOptions
{
    [Range(1, 100)]
    public int MaxFailedAttemptsPerWindow { get; init; } = 5;

    [Range(1, 1440)]
    public int FailedAttemptsWindowMinutes { get; init; } = 15;

    [Range(1, 3600)]
    public int CooldownSeconds { get; init; } = 60;

    [Range(1, 100)]
    public int MaxSuccessfulChangesPerWindow { get; init; } = 5;

    [Range(1, 168)]
    public int SuccessfulChangesWindowHours { get; init; } = 24;

    [Range(1, 1000)]
    public int IpPermitLimit { get; init; } = 20;

    [Range(1, 1440)]
    public int IpWindowMinutes { get; init; } = 1;
}
