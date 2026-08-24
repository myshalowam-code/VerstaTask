using Versta.Orders.Domain.Orders;

namespace Versta.Orders.Application.Abstractions;

public interface IOrderEventStore
{
    Task AppendAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> LoadAsync(OrderId id, CancellationToken cancellationToken);
}
