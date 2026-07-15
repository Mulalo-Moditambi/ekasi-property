using SharedKernel;

namespace Domain.Properties;

public sealed record PropertyDeletedDomainEvent(Guid PropertyId) : IDomainEvent;
