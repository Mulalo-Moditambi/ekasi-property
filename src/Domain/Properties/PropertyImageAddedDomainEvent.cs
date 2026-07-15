using SharedKernel;

namespace Domain.Properties;

public sealed record PropertyImageAddedDomainEvent(Guid PropertyImageId) : IDomainEvent;
