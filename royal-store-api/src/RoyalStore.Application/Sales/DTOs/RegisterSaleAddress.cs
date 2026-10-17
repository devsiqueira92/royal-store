namespace RoyalStore.Application.Sales.DTOs;

/// <summary>Optional delivery/pickup address. Every field is optional; normalization and limits live
/// in <c>SaleAddress.Create</c>.</summary>
public record RegisterSaleAddress(string? Street, string? Number, string? City, string? ZipCode);
