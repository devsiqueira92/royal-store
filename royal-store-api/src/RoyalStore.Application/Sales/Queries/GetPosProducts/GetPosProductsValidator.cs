using FluentValidation;

namespace RoyalStore.Application.Sales.Queries.GetPosProducts;

public sealed class GetPosProductsValidator : AbstractValidator<GetPosProductsQuery>
{
    public GetPosProductsValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("The 'category-id' filter must be a non-empty identifier.");
    }
}
