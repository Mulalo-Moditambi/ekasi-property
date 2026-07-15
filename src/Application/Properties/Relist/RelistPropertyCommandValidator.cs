using FluentValidation;

namespace Application.Properties.Relist;

public class RelistPropertyCommandValidator : AbstractValidator<RelistPropertyCommand>
{
    public RelistPropertyCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
    }
}
