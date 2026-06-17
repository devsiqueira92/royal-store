using FluentValidation;

namespace RoyalStore.Application.Catalog.Queries.GetCatalog;

public sealed class GetCatalogValidator : AbstractValidator<GetCatalogQuery>
{
    public GetCatalogValidator()
    {
        RuleFor(x => x.Since)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .When(x => x.Since.HasValue)
            .WithMessage("The 'since' parameter must not be in the future.");
    }
}
