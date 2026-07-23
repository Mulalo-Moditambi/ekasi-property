using Application.Abstractions.Billing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Billing;

internal sealed class SubscriptionSweepBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SubscriptionSweepBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TickInterval);

        do
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            ISubscriptionSweepService sweepService = scope.ServiceProvider.GetRequiredService<ISubscriptionSweepService>();

            try
            {
                await sweepService.RunOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Subscription sweep tick failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
