using SharedKernel;

namespace Domain.Inquiries;

public sealed record InquirySubmittedDomainEvent(Guid InquiryId) : IDomainEvent;
