namespace RoyalStore.Application.Sales.DTOs;

/// <summary>A registered customer selected at checkout, referenced by id — never by document or
/// phone (038-register-sale-checkout FR-008).</summary>
public record RegisterSaleCustomerRef(Guid Id);
