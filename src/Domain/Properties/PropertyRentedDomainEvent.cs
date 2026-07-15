using SharedKernel;

namespace Domain.Properties;

public sealed record PropertyRentedDomainEvent(Guid PropertyId) : IDomainEvent;
