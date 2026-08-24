using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Versta.Orders.Application.Orders.ListOrders;
using Versta.Orders.Domain.Common;

namespace Versta.Orders.Api.Errors;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException
            && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Клиент отменил HTTP-запрос {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
            return true;
        }

        switch (exception)
        {
            case DomainValidationException validationException:
                logger.LogWarning(
                    "Запрос отклонён доменом: {Field} — {Message}",
                    validationException.Field,
                    validationException.Message);
                await Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        [validationException.Field] = [validationException.Message]
                    },
                    title: "Ошибка валидации")
                    .ExecuteAsync(httpContext);
                return true;

            case DbUpdateConcurrencyException concurrencyException:
                logger.LogWarning(concurrencyException, "Конфликт конкурентного изменения заказа.");
                await Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Конфликт изменения заказа",
                    detail: concurrencyException.Message)
                    .ExecuteAsync(httpContext);
                return true;

            case InvalidOrderCursorException cursorException:
                logger.LogWarning(cursorException, "Получен некорректный cursor страницы заказов.");
                await Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        ["cursor"] = [cursorException.Message]
                    },
                    title: "Ошибка пагинации")
                    .ExecuteAsync(httpContext);
                return true;

            case UnauthorizedAccessException unauthorizedException:
                logger.LogWarning(unauthorizedException, "Запрос не авторизован.");
                await Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Требуется авторизация")
                    .ExecuteAsync(httpContext);
                return true;

            default:
                logger.LogError(exception, "Необработанная ошибка Orders API.");
                await Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Внутренняя ошибка сервера")
                    .ExecuteAsync(httpContext);
                return true;
        }
    }
}
