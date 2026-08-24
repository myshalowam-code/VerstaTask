using MediatR;
using Versta.Orders.Application.Abstractions;

namespace Versta.Orders.Application.Orders.GetOrder;

public sealed class GetOrderHandler(IOrderReadRepository repository)
    : IRequestHandler<GetOrderQuery, OrderDetails?>
{
    public Task<OrderDetails?> Handle(GetOrderQuery request, CancellationToken cancellationToken) =>
        repository.GetAsync(request.Id, request.CreatedBy, cancellationToken);
}
