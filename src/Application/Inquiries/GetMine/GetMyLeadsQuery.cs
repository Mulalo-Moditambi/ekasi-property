using Application.Abstractions.Messaging;

namespace Application.Inquiries.GetMine;

public sealed record GetMyLeadsQuery(
    Guid? PropertyId = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IQuery<GetMyLeadsResponse>;
