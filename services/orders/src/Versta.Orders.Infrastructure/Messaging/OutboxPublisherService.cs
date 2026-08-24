using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Versta.Orders.Infrastructure.Persistence;

namespace Versta.Orders.Infrastructure.Messaging;

public sealed class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxPublisherService> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var kafka = options.Value;
        using var producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = kafka.BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All
        }).Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatchAsync(producer, kafka.OrdersTopic, stoppingToken);
                await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Не удалось опубликовать outbox-сообщения в Kafka.");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }

    private async Task PublishBatchAsync(
        IProducer<string, string> producer,
        string topic,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
        var messages = await dbContext.Outbox
            .Where(x => x.PublishedAtUtc == null)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            var envelope = new OrderEventEnvelope(
                message.Id,
                message.StreamId,
                message.StreamVersion,
                message.EventType,
                message.Payload,
                message.OccurredAtUtc);

            await producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = message.StreamId,
                Value = JsonSerializer.Serialize(envelope, JsonOptions)
            }, cancellationToken);

            message.PublishedAtUtc = timeProvider.GetUtcNow();
        }

        if (messages.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);
    }
}
