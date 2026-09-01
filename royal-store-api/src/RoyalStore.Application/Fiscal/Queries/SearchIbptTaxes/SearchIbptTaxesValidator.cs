using FluentValidation;

namespace RoyalStore.Application.Fiscal.Queries.SearchIbptTaxes;

public sealed class SearchIbptTaxesValidator : AbstractValidator<SearchIbptTaxesQuery>
{
    public SearchIbptTaxesValidator()
    {
        RuleFor(x => x.Q)
            .MaximumLength(200)
            .When(x => x.Q is not null)
            .WithMessage("The search term must be at most 200 characters.");
    }
}
