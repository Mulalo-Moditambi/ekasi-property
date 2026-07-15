using SharedKernel;

namespace Domain.Properties;

public sealed record PropertyRelistedDomainEvent(Guid PropertyId) : IDomainEvent;
