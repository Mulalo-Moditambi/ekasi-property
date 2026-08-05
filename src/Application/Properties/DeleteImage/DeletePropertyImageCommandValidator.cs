using FluentValidation;

namespace Application.Properties.DeleteImage;

public class DeletePropertyImageCommandValidator : AbstractValidator<DeletePropertyImageCommand>
{
    public DeletePropertyImageCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
        RuleFor(c => c.ImageId).NotEmpty();
    }
}
