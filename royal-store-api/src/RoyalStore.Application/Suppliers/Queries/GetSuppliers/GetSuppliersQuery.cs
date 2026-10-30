using MediatR;
using RoyalStore.Application.Suppliers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Suppliers.Queries.GetSuppliers;

public record GetSuppliersQuery(bool? IsActive) : IRequest<Result<IReadOnlyList<SupplierResponse>>>;
