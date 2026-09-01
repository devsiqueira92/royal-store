using MediatR;
using RoyalStore.Application.Fiscal.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Fiscal.Queries.GetIbptTaxById;

public record GetIbptTaxByIdQuery(Guid Id)
    : IRequest<Result<IbptTaxResponse>>;
