using MongoDB.Driver;
using Versta.Orders.Application.Abstractions;
using Versta.Orders.Application.Orders;
using Versta.Orders.Application.Orders.ListOrders;
using Versta.Orders.Domain.Orders.Events;

namespace Versta.Orders.Infrastructure.ReadModel;

public sealed class MongoOrderReadRepository(IMongoDatabase database) : IOrderReadRepository
{
    private readonly IMongoCollection<OrderReadDocument> _orders =
        database.GetCollection<OrderReadDocument>("orders");

    public Task UpsertAsync(OrderCreated created, CancellationToken cancellationToken)
    {
        var document = new OrderReadDocument
        {
            Id = created.OrderId,
            Number = created.OrderNumber,
            SenderCity = created.SenderCity,
            SenderAddress = created.SenderAddress,
            RecipientCity = created.RecipientCity,
            RecipientAddress = created.RecipientAddress,
            WeightKg = created.WeightKg,
            PickupDate = created.PickupDate,
            CreatedAtUtc = created.OccurredAtUtc.UtcDateTime,
            CreatedBy = created.CreatedBy
        };

        return _orders.ReplaceOneAsync(
            x => x.Id == document.Id,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    public async Task<IReadOnlyList<OrderListItem>> ListAsync(
        Guid createdBy,
        int take,
        OrderPagePosition? position,
        CancellationToken cancellationToken)
    {
        var filters = Builders<OrderReadDocument>.Filter;
        var filter = filters.Eq(x => x.CreatedBy, createdBy);
        if (position is not null)
        {
            var createdAtUtc = position.CreatedAtUtc.UtcDateTime;
            filter &= filters.Or(
                filters.Lt(x => x.CreatedAtUtc, createdAtUtc),
                filters.And(
                    filters.Eq(x => x.CreatedAtUtc, createdAtUtc),
                    filters.Lt(x => x.Id, position.Id)));
        }

        var sort = Builders<OrderReadDocument>.Sort
            .Descending(x => x.CreatedAtUtc)
            .Descending(x => x.Id);
        var documents = await _orders.Find(filter)
            .Sort(sort)
            .Limit(take)
            .ToListAsync(cancellationToken);
        return documents.Select(MapListItem).ToList();
    }

    public async Task<OrderDetails?> GetAsync(Guid id, Guid createdBy, CancellationToken cancellationToken)
    {
        var document = await _orders.Find(x => x.Id == id && x.CreatedBy == createdBy)
            .FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : Map(document);
    }

    private static OrderDetails Map(OrderReadDocument x) => new(
        x.Id, x.Number, x.SenderCity, x.SenderAddress,
        x.RecipientCity, x.RecipientAddress, x.WeightKg,
        x.PickupDate, AsUtcOffset(x.CreatedAtUtc));

    private static OrderListItem MapListItem(OrderReadDocument x) => new(
        x.Id, x.Number, x.SenderCity, x.RecipientCity,
        x.WeightKg, x.PickupDate, AsUtcOffset(x.CreatedAtUtc));

    private static DateTimeOffset AsUtcOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
