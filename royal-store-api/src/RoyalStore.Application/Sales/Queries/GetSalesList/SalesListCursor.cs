namespace RoyalStore.Application.Sales.Queries.GetSalesList;

/// <summary>
/// Keyset key of the sales list: its sort columns, newest first by <see cref="SoldAt"/>, then by
/// <see cref="Id"/> so sales sold at the same instant keep a stable order (067 FR-010).
/// </summary>
public sealed record SalesListCursor(DateTime SoldAt, Guid Id);
