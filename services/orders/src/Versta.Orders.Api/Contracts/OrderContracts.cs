namespace Versta.Orders.Api.Contracts;

public sealed record CreateOrderRequest(
    string SenderCity,
    string SenderAddress,
    string RecipientCity,
    string RecipientAddress,
    decimal WeightKg,
    DateOnly PickupDate);

public sealed record CreateOrderResponse(Guid OrderId);
