using Versta.Orders.Domain.Common;

namespace Versta.Orders.Domain.Orders;

public sealed record Address
{
    public string City { get; }
    public string Street { get; }

    public Address(string city, string street)
    {
        City = Required(city, nameof(city));
        Street = Required(street, nameof(street));
    }

    private static string Required(string value, string field) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainValidationException(field, "Поле обязательно для заполнения.")
            : value.Trim();
}
