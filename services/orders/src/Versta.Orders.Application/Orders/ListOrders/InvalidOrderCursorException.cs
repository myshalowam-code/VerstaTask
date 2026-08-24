namespace Versta.Orders.Application.Orders.ListOrders;

public sealed class InvalidOrderCursorException()
    : Exception("Некорректный cursor страницы заказов.");
