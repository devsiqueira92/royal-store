using RoyalStore.Domain.Suppliers;

namespace RoyalStore.Application.Suppliers.DTOs;

public record SupplierResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Cnpj,
    string? Phone,
    string? Address,
    string? City,
    string? ZipCode,
    bool IsActive)
{
    public static SupplierResponse From(Supplier supplier) =>
        new(supplier.Id, supplier.TenantId, supplier.Name, supplier.Cnpj, supplier.Phone,
            supplier.Address, supplier.City, supplier.ZipCode, supplier.IsActive);
}
