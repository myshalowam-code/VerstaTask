using Versta.Orders.Application.Orders;
using Versta.Orders.Application.Orders.ListOrders;
using Versta.Orders.Domain.Orders.Events;

namespace Versta.Orders.Application.Abstractions;

public interface IOrderReadRepository
{
    Task UpsertAsync(OrderCreated created, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderListItem>> ListAsync(
        Guid createdBy,
        int take,
        OrderPagePosition? position,
        CancellationToken cancellationToken);
    Task<OrderDetails?> GetAsync(Guid id, Guid createdBy, CancellationToken cancellationToken);
}
