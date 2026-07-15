using FluentValidation;

namespace Application.Properties.Update;

public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Description).NotEmpty().MaximumLength(4000);
        RuleFor(c => c.Price).GreaterThan(0);
        RuleFor(c => c.Street).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Township).NotEmpty().MaximumLength(100);
        RuleFor(c => c.City).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Province).NotEmpty().MaximumLength(100);
        RuleFor(c => c.PostalCode).NotEmpty().MaximumLength(10);
        RuleFor(c => c.Bedrooms).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Bathrooms).GreaterThanOrEqualTo(0);
    }
}
