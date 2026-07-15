using FluentValidation;

namespace Application.Properties.AddImages;

public class AddPropertyImagesCommandValidator : AbstractValidator<AddPropertyImagesCommand>
{
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    public AddPropertyImagesCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
        RuleFor(c => c.Images).NotEmpty();
        RuleForEach(c => c.Images).ChildRules(image =>
        {
            image.RuleFor(i => i.Length)
                .LessThanOrEqualTo(MaxFileSizeInBytes)
                .WithMessage("Each image must be 5 MB or smaller");
            image.RuleFor(i => i.ContentType)
                .Must(contentType => AllowedContentTypes.Contains(contentType))
                .WithMessage("Only JPEG, PNG, and WebP images are allowed");
        });
    }
}
