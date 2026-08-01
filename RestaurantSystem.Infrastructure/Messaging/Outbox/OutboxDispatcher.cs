// Path: RestaurantSystem.Infrastructure/Messaging/Outbox/OutboxDispatcher.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Infrastructure.Persistence.EventStore;

namespace RestaurantSystem.Infrastructure.Messaging.Outbox;

public sealed class OutboxDispatcher(
    EventStoreDbContext db,
    IIntegrationEventPublisher publisher,
    IOptions<OutboxDispatcherOptions> options) : IOutboxDispatcher
{
    private readonly OutboxDispatcherOptions _options = options.Value;

    // در OutboxDispatcher.cs
    public async Task DispatchPendingAsync(CancellationToken ct)
    {
        var batchSize = _options.BatchSize;
        var workerId = Guid.NewGuid(); // شناسه‌ی یکتای این Worker
        var now = DateTime.UtcNow;

        // تکنیک: انتخاب و Lock کردن اتمیک با FOR UPDATE SKIP LOCKED
        // این Query به Postgres می‌گوید: ردیف‌های آزاد را بگیر و Lock کن، بقیه را Skip کن.
        // اصلاح کوئری در OutboxDispatcher
        string sql = @"
                UPDATE ""EventStore"".""OutboxMessages""
                SET ""ProcessingStartedAt"" = @p0, ""LockId"" = @p1
                WHERE ""Id"" IN (
                    SELECT ""Id"" FROM ""EventStore"".""OutboxMessages""
                    WHERE ""ProcessedOnUtc"" IS NULL 
                    AND ""Error"" IS NULL 
                    AND ""ProcessingStartedAt"" IS NULL
                    ORDER BY ""OccurredOnUtc""
                    LIMIT 10
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING *;";


        var messages = await db.OutboxMessages
            .FromSqlRaw(sql, now, workerId)
            .ToListAsync(ct);

        if (!messages.Any()) return;

        foreach (var m in messages)
        {
            try
            {
                var envelope = new IntegrationEventEnvelope(
                    MessageId: m.Id,
                    Type: m.Type,
                    PayloadJson: m.Payload,
                    OccurredOnUtc: m.OccurredOnUtc,
                    Headers: m.Headers
                );

                await publisher.PublishAsync(envelope, ct);

                m.ProcessedOnUtc = DateTime.UtcNow;
                m.ProcessingStartedAt = null;
            }
            catch (Exception ex)
            {
                m.Error = ex.ToString();
                m.ProcessingStartedAt = null;
            }
        }

        await db.SaveChangesAsync(ct);
    }

}
