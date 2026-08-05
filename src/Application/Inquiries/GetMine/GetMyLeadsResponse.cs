namespace Application.Inquiries.GetMine;

public sealed class GetMyLeadsResponse
{
    public List<LeadResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage { get; set; }
}
