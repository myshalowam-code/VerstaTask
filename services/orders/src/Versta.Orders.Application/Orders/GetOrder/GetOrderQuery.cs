using MediatR;

namespace Versta.Orders.Application.Orders.GetOrder;

public sealed record GetOrderQuery(Guid Id, Guid CreatedBy) : IRequest<OrderDetails?>;
