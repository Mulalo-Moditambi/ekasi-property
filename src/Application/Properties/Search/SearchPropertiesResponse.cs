namespace Application.Properties.Search;

public sealed class SearchPropertiesResponse
{
    public List<PropertySummaryResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage { get; set; }
}
