using FluentValidation;

namespace RoyalStore.Application.Common.Pagination;

/// <summary>Plugged into each paged query's validator with <c>SetValidator</c> (067 FR-005).</summary>
public sealed class PageRequestValidator : AbstractValidator<PageRequest>
{
    public PageRequestValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PageRequest.MaxPageSize)
            .When(x => x.PageSize.HasValue)
            .WithMessage($"The 'page-size' parameter must be between 1 and {PageRequest.MaxPageSize}.");
    }
}
