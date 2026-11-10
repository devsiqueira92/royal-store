using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Application.Options;

public sealed class ResendOptions
{
    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string FromEmail { get; init; } = string.Empty;

    public string FromName { get; init; } = "Royal Store";

    public bool SendRealEmails { get; init; } = false;
}
