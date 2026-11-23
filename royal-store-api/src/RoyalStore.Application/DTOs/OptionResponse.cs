namespace RoyalStore.Application.DTOs;

/// <summary>One entry of a selection list (dropdown): a value to send back and the text to show.</summary>
public sealed record OptionResponse(string Code, string Description);
