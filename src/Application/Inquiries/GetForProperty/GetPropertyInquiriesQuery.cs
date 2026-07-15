using Application.Abstractions.Messaging;

namespace Application.Inquiries.GetForProperty;

public sealed record GetPropertyInquiriesQuery(Guid PropertyId) : IQuery<List<InquiryResponse>>;
