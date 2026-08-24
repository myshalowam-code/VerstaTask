using Versta.Orders.Domain.Common;

namespace Versta.Orders.Domain.Orders;

public readonly record struct CargoWeight
{
    public decimal Kilograms { get; }

    public CargoWeight(decimal kilograms)
    {
        if (kilograms <= 0 || kilograms > 100_000)
            throw new DomainValidationException(
                nameof(kilograms),
                "Вес должен быть больше 0 и не превышать 100 000 кг.");

        Kilograms = decimal.Round(kilograms, 3);
    }
}
