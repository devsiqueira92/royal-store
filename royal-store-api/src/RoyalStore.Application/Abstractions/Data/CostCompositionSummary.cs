namespace RoyalStore.Application.Abstractions.Data;

/// <summary>
/// One row of the store's cost composition lists (064): everything a listing needs, with the number
/// of lines counted by the database instead of loading the lines.
/// </summary>
/// <param name="ProductId">The product the list is linked to; null when it is linked to none.</param>
public sealed record CostCompositionSummary(
    Guid Id,
    Guid? ProductId,
    string Name,
    decimal TotalValue,
    int NumberItems);
