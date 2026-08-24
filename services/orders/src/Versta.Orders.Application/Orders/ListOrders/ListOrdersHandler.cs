using System.Globalization;
using MediatR;
using Versta.Orders.Application.Abstractions;

namespace Versta.Orders.Application.Orders.ListOrders;

public sealed class ListOrdersHandler(IOrderReadRepository repository)
    : IRequestHandler<ListOrdersQuery, OrderPage>
{
    private const int DefaultLimit = 20;
    private const int MaximumLimit = 100;

    public async Task<OrderPage> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit <= 0
            ? DefaultLimit
            : Math.Min(request.Limit, MaximumLimit);
        var position = DecodeCursor(request.Cursor);
        var orders = await repository.ListAsync(
            request.CreatedBy,
            limit + 1,
            position,
            cancellationToken);
        var hasMore = orders.Count > limit;
        var items = orders.Take(limit).ToList();
        var nextCursor = hasMore ? EncodeCursor(items[^1]) : null;

        return new OrderPage(items, nextCursor, hasMore);
    }

    private static string EncodeCursor(OrderListItem order) =>
        $"{order.CreatedAtUtc.ToUnixTimeMilliseconds():x16}.{order.Id:N}";

    private static OrderPagePosition? DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return null;

        var parts = cursor.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2
            || !long.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var timestamp)
            || !Guid.TryParseExact(parts[1], "N", out var id))
            throw new InvalidOrderCursorException();

        try
        {
            return new OrderPagePosition(DateTimeOffset.FromUnixTimeMilliseconds(timestamp), id);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new InvalidOrderCursorException();
        }
    }
}
