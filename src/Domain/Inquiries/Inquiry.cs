using SharedKernel;

namespace Domain.Inquiries;

public sealed class Inquiry : Entity
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }

    public static Inquiry Create(
        Guid propertyId,
        string name,
        string email,
        string? phone,
        string message,
        DateTime utcNow)
    {
        var inquiry = new Inquiry
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            Name = name,
            Email = email,
            Phone = phone,
            Message = message,
            CreatedAt = utcNow
        };

        inquiry.Raise(new InquirySubmittedDomainEvent(inquiry.Id));

        return inquiry;
    }
}
