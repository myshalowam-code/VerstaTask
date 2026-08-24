using Versta.Orders.Domain.Common;

namespace Versta.Orders.Domain.Orders.Events;

public sealed record OrderCreated(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid OrderId,
    string OrderNumber,
    string SenderCity,
    string SenderAddress,
    string RecipientCity,
    string RecipientAddress,
    decimal WeightKg,
    DateOnly PickupDate,
    Guid CreatedBy) : DomainEvent(EventId, OccurredAtUtc);
