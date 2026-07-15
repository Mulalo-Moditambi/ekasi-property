using Application.Abstractions.Messaging;

namespace Application.Properties.GetById;

public sealed record GetPropertyByIdQuery(Guid PropertyId) : IQuery<PropertyResponse>;
