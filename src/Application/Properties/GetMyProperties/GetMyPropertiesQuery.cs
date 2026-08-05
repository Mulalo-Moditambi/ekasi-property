using Application.Abstractions.Messaging;
using Domain.Properties;

namespace Application.Properties.GetMyProperties;

public sealed record GetMyPropertiesQuery(
    PropertyStatus? Status = null,
    int Page = 1,
    int PageSize = 20,
    string? Sort = null) : IQuery<GetMyPropertiesResponse>;
