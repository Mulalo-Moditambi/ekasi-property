using SharedKernel;

namespace Domain.Properties;

public sealed record PropertyUpdatedDomainEvent(Guid PropertyId) : IDomainEvent;
