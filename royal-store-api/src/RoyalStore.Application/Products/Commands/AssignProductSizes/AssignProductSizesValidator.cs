using FluentValidation;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.Commands.AssignProductSizes;

public sealed class AssignProductSizesValidator : AbstractValidator<AssignProductSizesCommand>
{
    private static readonly HashSet<string> ValidSizes =
        Enum.GetNames<SizeValue>().ToHashSet(StringComparer.OrdinalIgnoreCase);

    public AssignProductSizesValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.Sizes)
            .NotNull()
            .ForEach(size => size
                .NotEmpty()
                .Must(s => ValidSizes.Contains(s))
                .WithMessage($"Size must be one of: {string.Join(", ", Enum.GetNames<SizeValue>())}."));

        RuleFor(x => x.Sizes)
            .Must(sizes => !ContainsOsfaWithOthers(sizes))
            .WithMessage("OSFA cannot be combined with other sizes.");
    }

    private static bool ContainsOsfaWithOthers(List<string> sizes) =>
        sizes.Any(s => string.Equals(s, nameof(SizeValue.Osfa), StringComparison.OrdinalIgnoreCase))
        && sizes.Count > 1;
}
