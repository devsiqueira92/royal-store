using FluentValidation;

namespace RoyalStore.Application.Products.Commands.AssignProductTags;

public sealed class AssignProductTagsValidator : AbstractValidator<AssignProductTagsCommand>
{
    public AssignProductTagsValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.TagIds).NotNull();

        RuleForEach(x => x.TagIds).NotEmpty();
    }
}
