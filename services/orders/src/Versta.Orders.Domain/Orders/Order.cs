using Versta.Orders.Domain.Common;
using Versta.Orders.Domain.Orders.Events;

namespace Versta.Orders.Domain.Orders;

public sealed class Order : AggregateRoot
{
    public OrderId Id { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public Address Sender { get; private set; } = null!;
    public Address Recipient { get; private set; } = null!;
    public CargoWeight Weight { get; private set; }
    public DateOnly PickupDate { get; private set; }
    public Guid CreatedBy { get; private set; }

    private Order() { }

    public static Order Create(
        Address sender,
        Address recipient,
        CargoWeight weight,
        DateOnly pickupDate,
        Guid createdBy,
        DateTimeOffset now)
    {
        if (pickupDate < DateOnly.FromDateTime(now.UtcDateTime))
            throw new DomainValidationException(
                nameof(pickupDate),
                "Дата забора не может быть в прошлом.");
        if (createdBy == Guid.Empty)
            throw new DomainValidationException(
                nameof(createdBy),
                "Пользователь не определён.");

        var id = OrderId.New();
        var order = new Order();
        order.Raise(new OrderCreated(
            Guid.NewGuid(), now, id.Value,
            $"ORD-{now:yyyyMMdd}-{id.Value.ToString("N")[..8].ToUpperInvariant()}",
            sender.City, sender.Street, recipient.City, recipient.Street,
            weight.Kilograms, pickupDate, createdBy));
        return order;
    }

    public static Order Rehydrate(IEnumerable<DomainEvent> history)
    {
        var order = new Order();
        order.LoadFromHistory(history);
        return order;
    }

    protected override void Apply(DomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case OrderCreated created:
                Id = new OrderId(created.OrderId);
                Number = created.OrderNumber;
                Sender = new Address(created.SenderCity, created.SenderAddress);
                Recipient = new Address(created.RecipientCity, created.RecipientAddress);
                Weight = new CargoWeight(created.WeightKg);
                PickupDate = created.PickupDate;
                CreatedBy = created.CreatedBy;
                break;
            default:
                throw new InvalidOperationException($"Неизвестное доменное событие {domainEvent.GetType().Name}.");
        }
    }
}
