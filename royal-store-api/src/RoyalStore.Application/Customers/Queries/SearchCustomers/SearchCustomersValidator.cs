using FluentValidation;

namespace RoyalStore.Application.Customers.Queries.SearchCustomers;

public sealed class SearchCustomersValidator : AbstractValidator<SearchCustomersQuery>
{
    /// <summary>The longest name a customer can have (Customers.Name is 150).</summary>
    private const int MaxSearchLength = 150;

    public SearchCustomersValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"The search term must not exceed {MaxSearchLength} characters.");
    }
}
