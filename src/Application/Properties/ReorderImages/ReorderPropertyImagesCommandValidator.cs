using FluentValidation;

namespace Application.Properties.ReorderImages;

public class ReorderPropertyImagesCommandValidator : AbstractValidator<ReorderPropertyImagesCommand>
{
    public ReorderPropertyImagesCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
        RuleFor(c => c.ImageIds).NotEmpty();
    }
}
