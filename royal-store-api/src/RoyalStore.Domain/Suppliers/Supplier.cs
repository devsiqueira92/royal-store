using RoyalStore.Domain.Common;
using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Suppliers;

public sealed class Supplier : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Cnpj { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? ZipCode { get; private set; }
    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Supplier() : base(Guid.NewGuid()) { }

    private Supplier(Guid id, Guid tenantId, string name)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        IsActive = true;
    }

    private static Result<(string Name, string? Cnpj, string? Phone, string? Address, string? City, string? ZipCode)>
        ValidateFields(string name, string? cnpj, string? phone, string? address, string? city, string? zipCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("supplier.name_required", "Supplier name is required.");

        name = name.Trim();
        if (name.Length > 150)
            return Error.Validation("supplier.name_too_long", "Supplier name must not exceed 150 characters.");

        if (!string.IsNullOrWhiteSpace(cnpj))
        {
            var normalizedCnpj = DocumentValidator.Normalize(cnpj);
            if (!DocumentValidator.IsValid(normalizedCnpj))
                return Error.Validation("supplier.invalid_cnpj", "CNPJ is invalid.");
            cnpj = normalizedCnpj;
        }
        else
        {
            cnpj = null;
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            phone = phone.Trim();
            if (phone.Length > 20)
                return Error.Validation("supplier.phone_too_long", "Phone must not exceed 20 characters.");
        }
        else
        {
            phone = null;
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            address = address.Trim();
            if (address.Length > 200)
                return Error.Validation("supplier.address_too_long", "Address must not exceed 200 characters.");
        }
        else
        {
            address = null;
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            city = city.Trim();
            if (city.Length > 100)
                return Error.Validation("supplier.city_too_long", "City must not exceed 100 characters.");
        }
        else
        {
            city = null;
        }

        if (!string.IsNullOrWhiteSpace(zipCode))
        {
            zipCode = zipCode.Trim();
            if (zipCode.Length > 10)
                return Error.Validation("supplier.zip_code_too_long", "Zip code must not exceed 10 characters.");
        }
        else
        {
            zipCode = null;
        }

        return (name, cnpj, phone, address, city, zipCode);
    }

    public static Result<Supplier> Create(
        Guid tenantId, string name, string? cnpj, string? phone, string? address, string? city, string? zipCode)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("supplier.tenant_required", "Tenant identifier is required.");

        var fields = ValidateFields(name, cnpj, phone, address, city, zipCode);
        if (!fields.IsSuccess)
            return fields.Error;

        var (n, c, p, a, ci, z) = fields.Value;

        var supplier = new Supplier(Guid.NewGuid(), tenantId, n)
        {
            Cnpj = c,
            Phone = p,
            Address = a,
            City = ci,
            ZipCode = z
        };

        return supplier;
    }

    public Result Update(string name, string? cnpj, string? phone, string? address, string? city, string? zipCode)
    {
        var fields = ValidateFields(name, cnpj, phone, address, city, zipCode);
        if (!fields.IsSuccess)
            return Result.Failure(fields.Error);

        var (n, c, p, a, ci, z) = fields.Value;

        Name = n;
        Cnpj = c;
        Phone = p;
        Address = a;
        City = ci;
        ZipCode = z;

        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(Error.Conflict("supplier.already_active", "Supplier is already active."));

        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("supplier.already_inactive", "Supplier is already inactive."));

        IsActive = false;
        return Result.Success();
    }
}
