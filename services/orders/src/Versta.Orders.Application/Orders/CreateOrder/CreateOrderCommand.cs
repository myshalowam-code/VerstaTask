using MediatR;

namespace Versta.Orders.Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    string SenderCity,
    string SenderAddress,
    string RecipientCity,
    string RecipientAddress,
    decimal WeightKg,
    DateOnly PickupDate,
    Guid CreatedBy) : IRequest<CreateOrderResult>;

public sealed record CreateOrderResult(Guid OrderId);
