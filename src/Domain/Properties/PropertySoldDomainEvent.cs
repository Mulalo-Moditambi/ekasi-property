using SharedKernel;

namespace Domain.Properties;

public sealed record PropertySoldDomainEvent(Guid PropertyId) : IDomainEvent;
