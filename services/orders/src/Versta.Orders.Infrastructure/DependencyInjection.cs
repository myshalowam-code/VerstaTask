using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Versta.Orders.Application.Abstractions;
using Versta.Orders.Infrastructure.Messaging;
using Versta.Orders.Infrastructure.Persistence;
using Versta.Orders.Infrastructure.ReadModel;
using Versta.Orders.Infrastructure.ReadModel.Migrations;

namespace Versta.Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventStore(configuration);
        services.AddReadModel(configuration);
        return services;
    }

    public static IServiceCollection AddEventStore(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.AddDbContext<EventStoreDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("EventStore")));
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddScoped<IOrderEventStore, PostgresOrderEventStore>();
        services.AddHostedService<OutboxPublisherService>();
        return services;
    }

    public static IServiceCollection AddReadModel(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        var mongoClient = new MongoClient(configuration.GetConnectionString("ReadModel"));
        services.AddSingleton<IMongoDatabase>(mongoClient.GetDatabase("versta_orders_read"));
        services.AddSingleton<MongoReadModelMigrator>();
        services.AddScoped<IOrderReadRepository, MongoOrderReadRepository>();
        return services;
    }

    public static async Task MigrateEventStoreAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("EventStoreMigrator");
        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();

        logger.LogInformation(
            "Применение {MigrationCount} миграций Event Store: {Migrations}.",
            pendingMigrations.Length,
            pendingMigrations);
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Миграции Event Store успешно применены.");
    }

    public static async Task MigrateReadModelAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider
            .GetRequiredService<MongoReadModelMigrator>()
            .MigrateAsync();
    }
}
