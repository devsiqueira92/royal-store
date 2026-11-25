using FluentValidation;
using RoyalStore.Application.Common.Pagination;

namespace RoyalStore.Application.Sales.Queries.GetSalesList;

public sealed class GetSalesListQueryValidator : AbstractValidator<GetSalesListQuery>
{
    public GetSalesListQueryValidator()
    {
        RuleFor(x => x.Page!)
            .SetValidator(new PageRequestValidator())
            .When(x => x.Page is not null);
    }
}
