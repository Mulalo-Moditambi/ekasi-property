using FluentValidation;

namespace Application.Properties.Withdraw;

public class WithdrawPropertyCommandValidator : AbstractValidator<WithdrawPropertyCommand>
{
    public WithdrawPropertyCommandValidator()
    {
        RuleFor(c => c.PropertyId).NotEmpty();
    }
}
