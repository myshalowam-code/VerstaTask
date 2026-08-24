using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Versta.Orders.Application.Abstractions;
using Versta.Orders.Domain.Orders.Events;
using Versta.Orders.Infrastructure.Messaging;

namespace Versta.Orders.Projection.Worker;

public sealed class OrderProjectionWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<OrderProjectionWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var kafka = options.Value;
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = kafka.BootstrapServers,
            GroupId = "orders-mongo-projection",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        }).Build();

        consumer.Subscribe(kafka.OrdersTopic);
        logger.LogInformation("Проекция MongoDB подписана на Kafka topic {Topic}.", kafka.OrdersTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    try
                    {
                        await ProjectAsync(result.Message.Value, stoppingToken);
                    }
                    catch (JsonException exception)
                    {
                        logger.LogError(exception, "Получено некорректное событие заказа; сообщение пропущено.");
                    }
                    consumer.Commit(result);
                }
                catch (ConsumeException exception)
                {
                    logger.LogError(exception, "Ошибка чтения события из Kafka.");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (KafkaException exception)
                {
                    logger.LogError(
                        exception,
                        "Ошибка подтверждения Kafka offset; событие будет безопасно обработано повторно.");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Проекция MongoDB остановлена.");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProjectAsync(string message, CancellationToken cancellationToken)
    {
        var envelope = JsonSerializer.Deserialize<OrderEventEnvelope>(message, JsonOptions)
            ?? throw new JsonException("Kafka envelope is empty.");

        if (envelope.EventType != nameof(OrderCreated))
        {
            logger.LogWarning("Неизвестный тип события {EventType} пропущен.", envelope.EventType);
            return;
        }

        var created = JsonSerializer.Deserialize<OrderCreated>(envelope.Payload, JsonOptions)
            ?? throw new JsonException("OrderCreated payload is empty.");

        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderReadRepository>();
        await repository.UpsertAsync(created, cancellationToken);
        logger.LogInformation(
            "Read model заказа {OrderId} обновлена из stream {StreamId}, version {Version}.",
            created.OrderId,
            envelope.StreamId,
            envelope.StreamVersion);
    }
}
