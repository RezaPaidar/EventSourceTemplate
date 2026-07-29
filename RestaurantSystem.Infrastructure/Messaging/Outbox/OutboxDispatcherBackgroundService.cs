using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RestaurantSystem.Infrastructure.Messaging.Outbox;

public class OutboxDispatcherBackgroundService(
    IServiceScopeFactory scopeFactory, IOptions<OutboxDispatcherOptions> options) : BackgroundService
{
    private readonly OutboxDispatcherOptions _options = options.Value; protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // PeriodicTimer is non-blocking and efficient
        using var timer = new PeriodicTimer(_options.Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();

            try
            {
                await dispatcher.DispatchPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"OutboxDispatcher failed: {ex.Message}");
            }
        }
    }
}
