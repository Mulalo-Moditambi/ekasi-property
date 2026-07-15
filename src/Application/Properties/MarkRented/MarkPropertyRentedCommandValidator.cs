using FluentValidation;

namespace Application.Properties.MarkRented;

public class MarkPropertyRentedCommandValidator : AbstractValidator<MarkPropertyRentedCommand>
{
    public MarkPropertyRentedCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
    }
}
