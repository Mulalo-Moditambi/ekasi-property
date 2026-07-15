using FluentValidation;

namespace Application.Properties.Create;

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(c => c.OwnerId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Description).NotEmpty().MaximumLength(4000);
        RuleFor(c => c.ListingType).IsInEnum();
        RuleFor(c => c.PropertyType).IsInEnum();
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
