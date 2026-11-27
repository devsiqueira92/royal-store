namespace RoyalStore.Application.Abstractions.Data;

/// <summary>
/// One row of the store's cost composition lists (064): everything a listing needs, with the number
/// of lines and of products using the list counted by the database instead of loaded.
/// </summary>
/// <param name="ProductsCount">How many products use the list; 0 when none does.</param>
public sealed record CostCompositionSummary(
    Guid Id,
    int ProductsCount,
    string Name,
    decimal TotalValue,
    int NumberItems);
