namespace Application.Inquiries.GetMine;

public sealed class LeadResponse
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyTitle { get; set; }
    public string PropertyTownship { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
