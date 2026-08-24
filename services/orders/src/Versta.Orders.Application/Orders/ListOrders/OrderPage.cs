namespace Versta.Orders.Application.Orders.ListOrders;

public sealed record OrderPage(
    IReadOnlyList<OrderListItem> Items,
    string? NextCursor,
    bool HasMore);
