using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Suppliers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Suppliers.Queries.GetSuppliers;

public sealed class GetSuppliersQueryHandler
    : IRequestHandler<GetSuppliersQuery, Result<IReadOnlyList<SupplierResponse>>>
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSuppliersQueryHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<IReadOnlyList<SupplierResponse>>> Handle(
        GetSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var suppliers = await _supplierRepository.GetAllAsync(request.IsActive, cancellationToken);
        IReadOnlyList<SupplierResponse> response = suppliers.Select(SupplierResponse.From).ToList();
        return Result<IReadOnlyList<SupplierResponse>>.Success(response);
    }
}
