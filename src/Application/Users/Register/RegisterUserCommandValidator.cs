using FluentValidation;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
        RuleFor(c => c.PhoneNumber).NotEmpty().Matches(@"^\+[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in E.164 format, e.g. +27821234567.");
    }
}
