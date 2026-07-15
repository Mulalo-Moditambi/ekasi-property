namespace Application.Inquiries.GetForProperty;

public sealed class InquiryResponse
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
