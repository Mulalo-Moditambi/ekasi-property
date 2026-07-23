using Application.Abstractions.Billing;
using Domain.Properties;
using Domain.Subscriptions;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Billing;

public sealed class SubscriptionSweepServiceTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task RunOnceAsync_Should_MoveExpiredTrialToGracePeriod()
    {
        // Arrange
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var owner = new User
        {
            Id = Guid.NewGuid(),
            Email = $"sweep-{Guid.NewGuid():N}@example.com",
            FirstName = "Sweep",
            LastName = "Test",
            PasswordHash = "hash",
            PhoneNumber = "+27821234567"
        };
        context.Users.Add(owner);

        var subscription = Subscription.StartTrial(owner.Id, DateTime.UtcNow.AddDays(-20));
        subscription.TrialEndsAt = DateTime.UtcNow.AddDays(-1);
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        ISubscriptionSweepService sweepService = scope.ServiceProvider.GetRequiredService<ISubscriptionSweepService>();

        // Act
        await sweepService.RunOnceAsync(CancellationToken.None);

        // Assert
        Subscription updated = await context.Subscriptions.SingleAsync(s => s.Id == subscription.Id);
        updated.Status.ShouldBe(SubscriptionStatus.GracePeriod);
    }

    [Fact]
    public async Task RunOnceAsync_Should_WithdrawSubscriptionAndListings_WhenGracePeriodHasLapsed()
    {
        // Arrange
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var owner = new User
        {
            Id = Guid.NewGuid(),
            Email = $"sweep-{Guid.NewGuid():N}@example.com",
            FirstName = "Sweep",
            LastName = "Test",
            PasswordHash = "hash",
            PhoneNumber = "+27821234567"
        };
        context.Users.Add(owner);

        var subscription = Subscription.StartTrial(owner.Id, DateTime.UtcNow.AddDays(-30));
        subscription.EnterGracePeriod(DateTime.UtcNow.AddDays(-8));
        context.Subscriptions.Add(subscription);

        var property = Property.Create(
            owner.Id,
            "Test listing",
            "Description",
            ListingType.Rent,
            PropertyType.Backroom,
            1500m,
            new Address("Street", "Township", "City", "Province", "0000"),
            1,
            1,
            true,
            true,
            true,
            false,
            DateTime.UtcNow);
        context.Properties.Add(property);

        await context.SaveChangesAsync();

        ISubscriptionSweepService sweepService = scope.ServiceProvider.GetRequiredService<ISubscriptionSweepService>();

        // Act
        await sweepService.RunOnceAsync(CancellationToken.None);

        // Assert
        Subscription updatedSubscription = await context.Subscriptions.SingleAsync(s => s.Id == subscription.Id);
        updatedSubscription.Status.ShouldBe(SubscriptionStatus.Withdrawn);

        Property updatedProperty = await context.Properties.SingleAsync(p => p.Id == property.Id);
        updatedProperty.Status.ShouldBe(PropertyStatus.Withdrawn);
    }
}
