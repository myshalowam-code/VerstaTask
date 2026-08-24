using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Versta.Orders.Infrastructure.ReadModel.Migrations;

public sealed class MongoReadModelMigrator(
    IMongoDatabase database,
    TimeProvider timeProvider,
    ILogger<MongoReadModelMigrator> logger)
{
    private const int OrdersPaginationMigration = 1;
    private readonly IMongoCollection<BsonDocument> _migrations =
        database.GetCollection<BsonDocument>("read_model_migrations");

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        var alreadyApplied = await _migrations
            .Find(Builders<BsonDocument>.Filter.Eq("_id", OrdersPaginationMigration))
            .AnyAsync(cancellationToken);
        if (alreadyApplied) return;

        await NormalizeCreatedAtUtcAsync(cancellationToken);
        await CreateOrdersPaginationIndexAsync(cancellationToken);

        await _migrations.InsertOneAsync(new BsonDocument
        {
            ["_id"] = OrdersPaginationMigration,
            ["Name"] = "OrdersPaginationIndex",
            ["AppliedAtUtc"] = timeProvider.GetUtcNow().UtcDateTime
        }, cancellationToken: cancellationToken);
        logger.LogInformation(
            "Mongo read-model migration {Version} ({Name}) применена.",
            OrdersPaginationMigration,
            "OrdersPaginationIndex");
    }

    private async Task NormalizeCreatedAtUtcAsync(CancellationToken cancellationToken)
    {
        var command = new BsonDocument
        {
            ["update"] = "orders",
            ["updates"] = new BsonArray
            {
                new BsonDocument
                {
                    ["q"] = new BsonDocument("CreatedAtUtc.DateTime", new BsonDocument("$exists", true)),
                    ["u"] = new BsonArray
                    {
                        new BsonDocument("$set", new BsonDocument("CreatedAtUtc", "$CreatedAtUtc.DateTime"))
                    },
                    ["multi"] = true
                }
            }
        };

        await database.RunCommandAsync(
            new BsonDocumentCommand<BsonDocument>(command),
            cancellationToken: cancellationToken);
    }

    private async Task CreateOrdersPaginationIndexAsync(CancellationToken cancellationToken)
    {
        var orders = database.GetCollection<OrderReadDocument>("orders");
        var keys = Builders<OrderReadDocument>.IndexKeys
            .Ascending(x => x.CreatedBy)
            .Descending(x => x.CreatedAtUtc)
            .Descending(x => x.Id);
        var index = new CreateIndexModel<OrderReadDocument>(keys, new CreateIndexOptions
        {
            Name = "IX_orders_CreatedBy_CreatedAtUtc_Id"
        });

        await orders.Indexes.CreateOneAsync(index, cancellationToken: cancellationToken);
    }
}
