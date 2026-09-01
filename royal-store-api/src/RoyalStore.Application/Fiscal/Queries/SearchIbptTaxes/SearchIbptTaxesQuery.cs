using MediatR;
using RoyalStore.Application.Fiscal.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Fiscal.Queries.SearchIbptTaxes;

public record SearchIbptTaxesQuery(string? Q)
    : IRequest<Result<IReadOnlyList<IbptTaxResponse>>>;
