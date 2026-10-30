using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Suppliers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Suppliers.Queries.GetSupplierById;

public sealed class GetSupplierByIdQueryHandler
    : IRequestHandler<GetSupplierByIdQuery, Result<SupplierResponse>>
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSupplierByIdQueryHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<SupplierResponse>> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier is null)
            return Error.NotFound("supplier.not_found", "Supplier not found.");

        return SupplierResponse.From(supplier);
    }
}
