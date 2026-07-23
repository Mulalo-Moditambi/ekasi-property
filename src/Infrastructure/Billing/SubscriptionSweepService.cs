using System.Data;
using System.Globalization;
using Application.Abstractions.Billing;
using Application.Abstractions.Notifications;
using Domain.Properties;
using Domain.Subscriptions;
using Domain.Users;
using Infrastructure.Database;
using Infrastructure.Notifications;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.Billing;

internal sealed class SubscriptionSweepService(
    ApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IWhatsAppSender whatsAppSender,
    IOptions<WhatsAppOptions> whatsAppOptions,
    ILogger<SubscriptionSweepService> logger)
    : ISubscriptionSweepService
{
    private const int SweepLockId = 987654321;

    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        bool lockAcquired = await TryAcquireLockAsync(cancellationToken);

        if (!lockAcquired)
        {
            return;
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        await ExpireTrialsAsync(utcNow, cancellationToken);
        await ExpireOverdueActiveSubscriptionsAsync(utcNow, cancellationToken);
        await SendGracePeriodRemindersAsync(utcNow, cancellationToken);
        await WithdrawLapsedSubscriptionsAsync(utcNow, cancellationToken);
    }

    private async Task<bool> TryAcquireLockAsync(CancellationToken cancellationToken)
    {
        var resultParameter = new SqlParameter("@Result", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "EXEC @Result = sp_getapplock @Resource = {0}, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0",
                [SweepLockId.ToString(CultureInfo.InvariantCulture), resultParameter],
                cancellationToken);

            int lockResult = (int)resultParameter.Value!;

            return lockResult >= 0;
        }
        catch (SqlException ex)
        {
            logger.LogInformation(ex, "Skipping subscription sweep tick; could not acquire the sweep lock");

            return false;
        }
    }

    private async Task ExpireTrialsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        List<Subscription> expiredTrials = await context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Trialing && s.TrialEndsAt <= utcNow)
            .ToListAsync(cancellationToken);

        foreach (Subscription subscription in expiredTrials)
        {
            subscription.EnterGracePeriod(utcNow);
        }

        if (expiredTrials.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ExpireOverdueActiveSubscriptionsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        DateTime overdueThreshold = utcNow.AddDays(-1);

        List<Subscription> overdue = await context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active &&
                        s.NextBillingDate != null &&
                        s.NextBillingDate < overdueThreshold)
            .ToListAsync(cancellationToken);

        foreach (Subscription subscription in overdue)
        {
            subscription.EnterGracePeriod(utcNow);
        }

        if (overdue.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SendGracePeriodRemindersAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        List<Subscription> inGracePeriod = await context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.GracePeriod)
            .ToListAsync(cancellationToken);

        foreach (Subscription subscription in inGracePeriod)
        {
            int daysInGrace = (utcNow.Date - subscription.GracePeriodStartedAt!.Value.Date).Days;

            int? dayToRemindFor = daysInGrace switch
            {
                1 when subscription.Day1ReminderSentAt is null => 1,
                4 when subscription.Day4ReminderSentAt is null => 4,
                6 when subscription.Day6ReminderSentAt is null => 6,
                _ => null
            };

            if (dayToRemindFor is null)
            {
                continue;
            }

            await SendReminderAsync(subscription, dayToRemindFor.Value, utcNow, cancellationToken);
        }
    }

    private async Task SendReminderAsync(
        Subscription subscription, int dayNumber, DateTime utcNow, CancellationToken cancellationToken)
    {
        User? owner = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == subscription.OwnerId, cancellationToken);

        if (owner is null || string.IsNullOrWhiteSpace(owner.PhoneNumber))
        {
            logger.LogWarning(
                "Skipping grace-period reminder for subscription {SubscriptionId}: owner has no phone number",
                subscription.Id);

            return;
        }

        int daysRemaining = 7 - dayNumber;

        Result sendResult = await whatsAppSender.SendTemplateMessageAsync(
            owner.PhoneNumber,
            whatsAppOptions.Value.ReminderTemplateName,
            [owner.FirstName, daysRemaining.ToString(CultureInfo.InvariantCulture)],
            cancellationToken);

        if (sendResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to send day-{DayNumber} grace-period reminder for subscription {SubscriptionId}",
                dayNumber,
                subscription.Id);

            return;
        }

        subscription.RecordReminderSent(dayNumber, utcNow);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task WithdrawLapsedSubscriptionsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        List<Subscription> lapsed = await context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.GracePeriod && s.GracePeriodEndsAt <= utcNow)
            .ToListAsync(cancellationToken);

        foreach (Subscription subscription in lapsed)
        {
            Result result = subscription.MarkWithdrawn(utcNow);

            if (result.IsFailure)
            {
                continue;
            }

            List<Property> activeListings = await context.Properties
                .Where(p => p.OwnerId == subscription.OwnerId && p.Status == PropertyStatus.Listed)
                .ToListAsync(cancellationToken);

            foreach (Property property in activeListings)
            {
                property.Withdraw(utcNow);
            }
        }

        if (lapsed.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
