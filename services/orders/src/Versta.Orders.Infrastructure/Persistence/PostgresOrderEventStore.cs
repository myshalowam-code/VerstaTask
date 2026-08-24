using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Versta.Orders.Application.Abstractions;
using Versta.Orders.Domain.Common;
using Versta.Orders.Domain.Orders;
using Versta.Orders.Domain.Orders.Events;

namespace Versta.Orders.Infrastructure.Persistence;

public sealed class PostgresOrderEventStore(EventStoreDbContext dbContext) : IOrderEventStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task AppendAsync(Order order, CancellationToken cancellationToken)
    {
        var streamId = OrderStream.Id(order.Id);
        var expectedVersion = order.Version;
        var actualVersion = await dbContext.Events
            .Where(x => x.StreamId == streamId)
            .Select(x => (int?)x.StreamVersion)
            .MaxAsync(cancellationToken) ?? 0;

        if (actualVersion != expectedVersion)
            throw new DbUpdateConcurrencyException("Агрегат заказа был изменен параллельно.");

        var version = expectedVersion;
        foreach (var domainEvent in order.UncommittedEvents)
        {
            var streamVersion = ++version;
            var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);
            dbContext.Events.Add(new StoredEvent
            {
                Id = domainEvent.EventId,
                StreamId = streamId,
                StreamType = OrderStream.Type,
                StreamVersion = streamVersion,
                EventType = domainEvent.GetType().Name,
                Payload = payload,
                OccurredAtUtc = domainEvent.OccurredAtUtc
            });
            dbContext.Outbox.Add(new OutboxMessage
            {
                Id = domainEvent.EventId,
                StreamId = streamId,
                StreamVersion = streamVersion,
                EventType = domainEvent.GetType().Name,
                Payload = payload,
                OccurredAtUtc = domainEvent.OccurredAtUtc
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        order.MarkChangesAsCommitted();
    }

    public async Task<Order?> LoadAsync(OrderId id, CancellationToken cancellationToken)
    {
        var streamId = OrderStream.Id(id);
        var records = await dbContext.Events.AsNoTracking()
            .Where(x => x.StreamId == streamId)
            .OrderBy(x => x.StreamVersion)
            .ToListAsync(cancellationToken);

        if (records.Count == 0) return null;
        return Order.Rehydrate(records.Select(Deserialize));
    }

    private static DomainEvent Deserialize(StoredEvent storedEvent) => storedEvent.EventType switch
    {
        nameof(OrderCreated) => JsonSerializer.Deserialize<OrderCreated>(storedEvent.Payload, JsonOptions)
            ?? throw new InvalidOperationException("Не удалось десериализовать OrderCreated."),
        _ => throw new InvalidOperationException($"Неизвестный тип события {storedEvent.EventType}.")
    };
}
