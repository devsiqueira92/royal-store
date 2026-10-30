using MediatR;
using RoyalStore.Application.Suppliers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Suppliers.Queries.GetSupplierById;

public record GetSupplierByIdQuery(Guid Id) : IRequest<Result<SupplierResponse>>;
