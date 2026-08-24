using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Versta.Orders.Application.Orders;
using Versta.Orders.Application.Orders.CreateOrder;
using Versta.Orders.Application.Orders.GetOrder;
using Versta.Orders.Application.Orders.ListOrders;
using Versta.Orders.Api.Contracts;

namespace Versta.Orders.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController(
    ISender sender,
    ILogger<OrdersController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateOrderResponse>(StatusCodes.Status202Accepted)]
    public async Task<ActionResult<CreateOrderResponse>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await sender.Send(new CreateOrderCommand(
            request.SenderCity,
            request.SenderAddress,
            request.RecipientCity,
            request.RecipientAddress,
            request.WeightKg,
            request.PickupDate,
            userId), cancellationToken);

        logger.LogInformation(
            "Команда создания заказа {OrderId} принята для пользователя {UserId}.",
            result.OrderId,
            userId);
        return Accepted(
            $"/api/orders/{result.OrderId}",
            new CreateOrderResponse(result.OrderId));
    }

    [HttpGet]
    [ProducesResponseType<OrderPage>(StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderPage>> List(
        [FromQuery] int limit = 20,
        [FromQuery] string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var page = await sender.Send(
            new ListOrdersQuery(userId, limit, cursor),
            cancellationToken);
        logger.LogDebug(
            "Пользователю {UserId} возвращена страница из {Count} заказов; HasMore={HasMore}.",
            userId,
            page.Items.Count,
            page.HasMore);
        return Ok(page);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetails>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var order = await sender.Send(
            new GetOrderQuery(id, userId),
            cancellationToken);
        if (order is not null) return Ok(order);

        logger.LogWarning(
            "Заказ {OrderId} не найден для пользователя {UserId}.",
            id,
            userId);
        return NotFound();
    }

    private Guid GetUserId() =>
        Guid.TryParse(User.FindFirstValue("sub"), out var id)
            ? id
            : throw new UnauthorizedAccessException("JWT does not contain a valid subject.");
}
