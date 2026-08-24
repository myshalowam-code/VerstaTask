using Versta.Orders.Domain.Orders;

namespace Versta.Orders.Infrastructure.Persistence;

public static class OrderStream
{
    public const string Type = "Order";
    public static string Id(OrderId orderId) => $"order-{orderId.Value:N}";
}
