using System.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Versta.Auth.Api.Data;
using Versta.Auth.Api.Models;
using Versta.Auth.Api.Services;

var migrateOnly = args.Any(argument =>
    string.Equals(argument, "--migrate-only", StringComparison.OrdinalIgnoreCase));
var appArguments = args
    .Where(argument => !string.Equals(
        argument,
        "--migrate-only",
        StringComparison.OrdinalIgnoreCase))
    .ToArray();
var builder = WebApplication.CreateBuilder(appArguments);
builder.Services.AddControllers();
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Auth")));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

var app = builder.Build();
if (migrateOnly)
{
    await app.Services.MigrateAuthDatabaseAsync();
    return;
}

app.UseHttpLogging();
app.UseExceptionHandler();
app.MapControllers();

await app.Services.SeedAuthDatabaseAsync();
app.Run();

public partial class Program;
