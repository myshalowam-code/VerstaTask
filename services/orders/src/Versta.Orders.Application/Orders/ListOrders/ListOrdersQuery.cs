using MediatR;

namespace Versta.Orders.Application.Orders.ListOrders;

public sealed record ListOrdersQuery(
    Guid CreatedBy,
    int Limit,
    string? Cursor) : IRequest<OrderPage>;
