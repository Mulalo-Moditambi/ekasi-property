namespace Application.Properties.GetMyProperties;

public sealed class GetMyPropertiesResponse
{
    public List<MyPropertyResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage { get; set; }
}
