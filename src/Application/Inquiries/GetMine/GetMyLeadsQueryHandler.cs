using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Inquiries.GetMine;

internal sealed class GetMyLeadsQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetMyLeadsQuery, GetMyLeadsResponse>
{
    private const int MaxPageSize = 50;

    public async Task<Result<GetMyLeadsResponse>> Handle(GetMyLeadsQuery query, CancellationToken cancellationToken)
    {
        int page = Math.Max(query.Page, 1);
        int pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var leads =
            from inquiry in context.Inquiries
            join property in context.Properties on inquiry.PropertyId equals property.Id
            where property.OwnerId == userContext.UserId
            select new { inquiry, property };

        if (query.PropertyId.HasValue)
        {
            leads = leads.Where(x => x.inquiry.PropertyId == query.PropertyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            leads = leads.Where(x =>
                x.inquiry.Name.Contains(query.Search) ||
                x.inquiry.Email.Contains(query.Search) ||
                x.inquiry.Message.Contains(query.Search));
        }

        int totalCount = await leads.CountAsync(cancellationToken);

        List<LeadResponse> items = await leads
            .OrderByDescending(x => x.inquiry.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LeadResponse
            {
                Id = x.inquiry.Id,
                PropertyId = x.inquiry.PropertyId,
                PropertyTitle = x.property.Title,
                PropertyTownship = x.property.Address.Township,
                Name = x.inquiry.Name,
                Email = x.inquiry.Email,
                Phone = x.inquiry.Phone,
                Message = x.inquiry.Message,
                CreatedAt = x.inquiry.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetMyLeadsResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            HasNextPage = page * pageSize < totalCount
        };
    }
}
