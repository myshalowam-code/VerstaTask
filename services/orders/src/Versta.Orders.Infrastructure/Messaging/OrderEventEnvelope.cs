namespace Versta.Orders.Infrastructure.Messaging;

public sealed record OrderEventEnvelope(
    Guid EventId,
    string StreamId,
    int StreamVersion,
    string EventType,
    string Payload,
    DateTimeOffset OccurredAtUtc);
