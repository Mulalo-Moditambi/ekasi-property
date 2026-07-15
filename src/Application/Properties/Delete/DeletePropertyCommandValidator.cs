using FluentValidation;

namespace Application.Properties.Delete;

public class DeletePropertyCommandValidator : AbstractValidator<DeletePropertyCommand>
{
    public DeletePropertyCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
    }
}
