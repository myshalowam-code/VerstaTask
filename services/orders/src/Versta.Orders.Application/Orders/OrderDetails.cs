namespace Versta.Orders.Application.Orders;

public sealed record OrderDetails(
    Guid Id,
    string Number,
    string SenderCity,
    string SenderAddress,
    string RecipientCity,
    string RecipientAddress,
    decimal WeightKg,
    DateOnly PickupDate,
    DateTimeOffset CreatedAtUtc);
