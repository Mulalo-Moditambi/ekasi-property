using FluentValidation;

namespace Application.Inquiries.Submit;

public class SubmitInquiryCommandValidator : AbstractValidator<SubmitInquiryCommand>
{
    public SubmitInquiryCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(c => c.Phone).MaximumLength(20);
        RuleFor(c => c.Message).NotEmpty().MaximumLength(2000);
    }
}
