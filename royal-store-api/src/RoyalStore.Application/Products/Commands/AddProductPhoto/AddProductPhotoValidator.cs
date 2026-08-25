using FluentValidation;

namespace RoyalStore.Application.Products.Commands.AddProductPhoto;

public sealed class AddProductPhotoValidator : AbstractValidator<AddProductPhotoCommand>
{
    public AddProductPhotoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Url must be a valid absolute URI.");
    }
}
