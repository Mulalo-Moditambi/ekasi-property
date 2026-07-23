using Domain.Inquiries;
using Domain.Properties;
using Domain.Subscriptions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Property> Properties { get; }
    DbSet<PropertyImage> PropertyImages { get; }
    DbSet<Inquiry> Inquiries { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<SubscriptionPayment> SubscriptionPayments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
