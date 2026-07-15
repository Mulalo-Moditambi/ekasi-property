using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Inquiries.GetForProperty;

internal sealed class GetPropertyInquiriesQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetPropertyInquiriesQuery, List<InquiryResponse>>
{
    public async Task<Result<List<InquiryResponse>>> Handle(
        GetPropertyInquiriesQuery query,
        CancellationToken cancellationToken)
    {
        bool ownsProperty = await context.Properties.AsNoTracking()
            .AnyAsync(p => p.Id == query.PropertyId && p.OwnerId == userContext.UserId, cancellationToken);

        if (!ownsProperty)
        {
            return Result.Failure<List<InquiryResponse>>(PropertyErrors.NotFound(query.PropertyId));
        }

        List<InquiryResponse> inquiries = await context.Inquiries
            .Where(i => i.PropertyId == query.PropertyId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InquiryResponse
            {
                Id = i.Id,
                PropertyId = i.PropertyId,
                Name = i.Name,
                Email = i.Email,
                Phone = i.Phone,
                Message = i.Message,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return inquiries;
    }
}
