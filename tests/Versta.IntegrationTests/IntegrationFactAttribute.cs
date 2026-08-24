namespace Versta.IntegrationTests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IntegrationFactAttribute : FactAttribute
{
    public IntegrationFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("GATEWAY_BASE_URL")))
        {
            Skip = "Запускается через ./scripts/run-integration-tests.sh.";
        }
    }
}
