namespace Versta.Orders.Domain.Common;

public sealed class DomainValidationException(
    string field,
    string message) : Exception(message)
{
    public string Field { get; } = field;
}
