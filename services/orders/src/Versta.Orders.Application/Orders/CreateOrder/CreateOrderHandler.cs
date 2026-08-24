using MediatR;
using Versta.Orders.Application.Abstractions;
using Versta.Orders.Domain.Orders;

namespace Versta.Orders.Application.Orders.CreateOrder;

public sealed class CreateOrderHandler(
    IOrderEventStore eventStore,
    TimeProvider timeProvider) : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(
            new Address(request.SenderCity, request.SenderAddress),
            new Address(request.RecipientCity, request.RecipientAddress),
            new CargoWeight(request.WeightKg),
            request.PickupDate,
            request.CreatedBy,
            timeProvider.GetUtcNow());

        await eventStore.AppendAsync(order, cancellationToken);

        return new CreateOrderResult(order.Id.Value);
    }
}
