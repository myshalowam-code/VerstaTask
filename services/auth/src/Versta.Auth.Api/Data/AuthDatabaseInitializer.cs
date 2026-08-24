using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Versta.Auth.Api.Models;

namespace Versta.Auth.Api.Data;

public static class AuthDatabaseInitializer
{
    public static async Task MigrateAuthDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AuthDatabaseInitializer");

        logger.LogInformation("Запуск миграций Auth.");
        await dbContext.Database.MigrateAsync();
        var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToArray();
        logger.LogInformation(
            "Миграции Auth успешно применены. Всего применено: {MigrationCount}; {Migrations}.",
            appliedMigrations.Length,
            appliedMigrations);
    }

    public static async Task SeedAuthDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AuthDatabaseInitializer");
        if (await dbContext.Users.AnyAsync()) return;

        var user = new User { Id = Guid.NewGuid(), Email = "demo@versta.local" };
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        user.PasswordHash = hasher.HashPassword(user, "Demo123!");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        logger.LogInformation(
            "Создан демонстрационный пользователь {UserId} с email {Email}.",
            user.Id,
            user.Email);
    }
}
