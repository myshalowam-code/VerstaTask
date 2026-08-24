namespace Versta.Auth.Api.Contracts;

public sealed record Credentials(string Email, string Password);

public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);
