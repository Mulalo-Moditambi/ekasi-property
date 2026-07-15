using Application.Inquiries.Submit;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Inquiries;

public sealed class InquiryValidatorsTests
{
    private readonly SubmitInquiryCommandValidator _submitValidator = new();

    private static SubmitInquiryCommand ValidCommand => new()
    {
        PropertyId = Guid.NewGuid(),
        Name = "Thabo Mokoena",
        Email = "thabo@example.com",
        Phone = "0721234567",
        Message = "Is the backroom still available?"
    };

    [Fact]
    public void SubmitValidator_Should_NotHaveErrors_WhenCommandIsValid()
    {
        TestValidationResult<SubmitInquiryCommand> result = _submitValidator.TestValidate(ValidCommand);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SubmitValidator_Should_NotHaveErrors_WhenPhoneIsMissing()
    {
        SubmitInquiryCommand command = ValidCommand;
        command.Phone = null;

        TestValidationResult<SubmitInquiryCommand> result = _submitValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SubmitValidator_Should_HaveError_WhenPropertyIdIsEmpty()
    {
        SubmitInquiryCommand command = ValidCommand;
        command.PropertyId = Guid.Empty;

        TestValidationResult<SubmitInquiryCommand> result = _submitValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.PropertyId);
    }

    [Fact]
    public void SubmitValidator_Should_HaveError_WhenNameIsEmpty()
    {
        SubmitInquiryCommand command = ValidCommand;
        command.Name = string.Empty;

        TestValidationResult<SubmitInquiryCommand> result = _submitValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void SubmitValidator_Should_HaveError_WhenEmailIsInvalid()
    {
        SubmitInquiryCommand command = ValidCommand;
        command.Email = "not-an-email";

        TestValidationResult<SubmitInquiryCommand> result = _submitValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void SubmitValidator_Should_HaveError_WhenMessageIsEmpty()
    {
        SubmitInquiryCommand command = ValidCommand;
        command.Message = string.Empty;

        TestValidationResult<SubmitInquiryCommand> result = _submitValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Message);
    }
}
