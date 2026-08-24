namespace Versta.Orders.Application.Orders;

public sealed record OrderListItem(
    Guid Id,
    string Number,
    string SenderCity,
    string RecipientCity,
    decimal WeightKg,
    DateOnly PickupDate,
    DateTimeOffset CreatedAtUtc);
