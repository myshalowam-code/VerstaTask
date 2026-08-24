using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Versta.Auth.Api.Contracts;
using Versta.Auth.Api.Data;
using Versta.Auth.Api.Models;
using Versta.Auth.Api.Services;

namespace Versta.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AuthDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    JwtTokenService tokenService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TokenResponse>> Register(
        Credentials request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email) || request.Password.Length < 8)
        {
            ModelState.AddModelError(
                "credentials",
                "Укажите email и пароль длиной не менее 8 символов.");
            return ValidationProblem(ModelState);
        }

        if (await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            logger.LogWarning("Попытка повторной регистрации email {Email}.", email);
            return Conflict(new { message = "Пользователь с таким email уже существует." });
        }

        var user = new User { Id = Guid.NewGuid(), Email = email };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Зарегистрирован пользователь {UserId} с email {Email}.",
            user.Id,
            user.Email);
        return Ok(tokenService.Create(user));
    }

    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login(
        Credentials request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users
            .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null || passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password) == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Неуспешная попытка входа для email {Email}.", email);
            return Unauthorized();
        }

        logger.LogInformation(
            "Пользователь {UserId} успешно вошёл в систему.",
            user.Id);
        return Ok(tokenService.Create(user));
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}
