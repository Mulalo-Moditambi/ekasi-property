using SharedKernel;

namespace Domain.Properties;

public sealed record PropertyWithdrawnDomainEvent(Guid PropertyId) : IDomainEvent;
