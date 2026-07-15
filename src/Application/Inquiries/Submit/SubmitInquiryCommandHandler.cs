using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Inquiries;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Inquiries.Submit;

internal sealed class SubmitInquiryCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<SubmitInquiryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SubmitInquiryCommand command, CancellationToken cancellationToken)
    {
        Property? property = await context.Properties.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == command.PropertyId, cancellationToken);

        if (property is null)
        {
            return Result.Failure<Guid>(PropertyErrors.NotFound(command.PropertyId));
        }

        if (property.Status != PropertyStatus.Listed)
        {
            return Result.Failure<Guid>(PropertyErrors.NotListed(command.PropertyId));
        }

        var inquiry = Inquiry.Create(
            property.Id,
            command.Name,
            command.Email,
            command.Phone,
            command.Message,
            dateTimeProvider.UtcNow);

        context.Inquiries.Add(inquiry);

        await context.SaveChangesAsync(cancellationToken);

        return inquiry.Id;
    }
}
