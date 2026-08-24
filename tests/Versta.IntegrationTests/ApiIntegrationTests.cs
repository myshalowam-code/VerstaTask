using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Sdk;

namespace Versta.IntegrationTests;

public sealed class ApiIntegrationTests : IAsyncLifetime
{
    private const string DemoEmail = "demo@versta.local";
    private const string DemoPassword = "Demo123!";
    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri(
            Environment.GetEnvironmentVariable("GATEWAY_BASE_URL")
            ?? "http://localhost:5080"),
        Timeout = TimeSpan.FromSeconds(5)
    };

    public Task InitializeAsync() => WaitForGatewayAsync();

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [IntegrationFact]
    public async Task Orders_endpoint_requires_authentication()
    {
        using var response = await _client.GetAsync("/api/orders?limit=1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [IntegrationFact]
    public async Task Login_rejects_invalid_password()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new Credentials(DemoEmail, "invalid-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [IntegrationFact]
    public async Task Invalid_order_is_rejected_by_domain_validation()
    {
        var accessToken = await LoginAsync();
        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            "/api/orders",
            new CreateOrderRequest(
                "Москва",
                "Тверская, 1",
                "Казань",
                "Баумана, 2",
                0,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))),
            accessToken);

        using var response = await _client.SendAsync(request);
        var problem = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var problemDocument = JsonDocument.Parse(problem);
        var errors = problemDocument.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("kilograms", out var weightErrors));
        var weightError = weightErrors[0].GetString();
        Assert.NotNull(weightError);
        Assert.Contains("Вес должен быть больше 0", weightError);
    }

    [IntegrationFact]
    public async Task Created_order_is_eventually_available_in_read_model()
    {
        var accessToken = await LoginAsync();
        var pickupDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        using var createRequest = CreateAuthorizedRequest(
            HttpMethod.Post,
            "/api/orders",
            new CreateOrderRequest(
                "Москва",
                "Тверская, 1",
                "Казань",
                "Баумана, 2",
                2.5m,
                pickupDate),
            accessToken);

        using var createResponse = await _client.SendAsync(createRequest);
        var createJson = await createResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);
        using var createDocument = JsonDocument.Parse(createJson);
        Assert.Single(createDocument.RootElement.EnumerateObject());
        var orderId = createDocument.RootElement.GetProperty("orderId").GetGuid();
        var location = Assert.IsType<Uri>(createResponse.Headers.Location);
        Assert.EndsWith($"/api/orders/{orderId}", location.ToString());

        var order = await WaitForOrderProjectionAsync(orderId, accessToken);
        Assert.Equal(orderId, order.Id);
        Assert.Equal("Москва", order.SenderCity);
        Assert.Equal("Казань", order.RecipientCity);
        Assert.Equal(2.5m, order.WeightKg);
        Assert.Equal(pickupDate, order.PickupDate);

        using var listRequest = CreateAuthorizedRequest(
            HttpMethod.Get,
            "/api/orders?limit=20",
            body: null,
            accessToken: accessToken);
        using var listResponse = await _client.SendAsync(listRequest);
        var page = await listResponse.Content.ReadFromJsonAsync<OrderPage>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(page);
        Assert.Contains(page.Items, item => item.Id == orderId);
    }

    private async Task WaitForGatewayAsync()
    {
        Exception? lastException = null;
        for (var attempt = 0; attempt < 120; attempt++)
        {
            try
            {
                using var response = await _client.GetAsync("/health");
                if (response.IsSuccessStatusCode) return;
            }
            catch (HttpRequestException exception)
            {
                lastException = exception;
            }
            catch (TaskCanceledException exception)
            {
                lastException = exception;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        throw new XunitException($"Gateway не стал доступен за 60 секунд: {lastException?.Message}");
    }

    private async Task<string> LoginAsync()
    {
        string? lastResponse = null;
        for (var attempt = 0; attempt < 60; attempt++)
        {
            using var response = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new Credentials(DemoEmail, DemoPassword));
            lastResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var token = JsonSerializer.Deserialize<TokenResponse>(
                    lastResponse,
                    JsonSerializerOptions.Web);
                return token?.AccessToken
                    ?? throw new XunitException("Auth API не вернул accessToken.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        throw new XunitException($"Auth API не стал готов за 30 секунд: {lastResponse}");
    }

    private async Task<OrderDetails> WaitForOrderProjectionAsync(Guid orderId, string accessToken)
    {
        string? lastResponse = null;
        for (var attempt = 0; attempt < 120; attempt++)
        {
            using var request = CreateAuthorizedRequest(
                HttpMethod.Get,
                $"/api/orders/{orderId}",
                body: null,
                accessToken: accessToken);
            using var response = await _client.SendAsync(request);
            lastResponse = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<OrderDetails>(
                           lastResponse,
                           JsonSerializerOptions.Web)
                       ?? throw new XunitException("Orders API вернул пустую read model.");
            }

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        throw new XunitException(
            $"Заказ {orderId} не появился в MongoDB за 60 секунд: {lastResponse}");
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string path,
        object? body,
        string accessToken)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = body is null ? null : JsonContent.Create(body)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private sealed record Credentials(string Email, string Password);

    private sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);

    private sealed record CreateOrderRequest(
        string SenderCity,
        string SenderAddress,
        string RecipientCity,
        string RecipientAddress,
        decimal WeightKg,
        DateOnly PickupDate);

    private sealed record OrderDetails(
        Guid Id,
        string Number,
        string SenderCity,
        string SenderAddress,
        string RecipientCity,
        string RecipientAddress,
        decimal WeightKg,
        DateOnly PickupDate,
        DateTimeOffset CreatedAtUtc);

    private sealed record OrderListItem(
        Guid Id,
        string Number,
        string SenderCity,
        string RecipientCity,
        decimal WeightKg,
        DateOnly PickupDate,
        DateTimeOffset CreatedAtUtc);

    private sealed record OrderPage(
        IReadOnlyList<OrderListItem> Items,
        string? NextCursor,
        bool HasMore);
}
