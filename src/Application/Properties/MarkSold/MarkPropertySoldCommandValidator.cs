using FluentValidation;

namespace Application.Properties.MarkSold;

public class MarkPropertySoldCommandValidator : AbstractValidator<MarkPropertySoldCommand>
{
    public MarkPropertySoldCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
    }
}
