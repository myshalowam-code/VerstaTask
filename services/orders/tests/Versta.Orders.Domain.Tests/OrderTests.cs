using Versta.Orders.Domain.Common;
using Versta.Orders.Domain.Orders;
using Versta.Orders.Domain.Orders.Events;

namespace Versta.Orders.Domain.Tests;

public sealed class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 24, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_RaisesOrderCreatedWithGeneratedNumber()
    {
        var order = Order.Create(
            new Address("Санкт-Петербург", "Невский, 1"),
            new Address("Москва", "Тверская, 1"),
            new CargoWeight(12.5m),
            new DateOnly(2026, 8, 25),
            Guid.NewGuid(),
            Now);

        var created = Assert.IsType<OrderCreated>(Assert.Single(order.UncommittedEvents));
        Assert.StartsWith("ORD-20260824-", created.OrderNumber);
        Assert.Equal(12.5m, created.WeightKg);
    }

    [Fact]
    public void Create_RejectsPickupDateInPast()
    {
        var exception = Assert.Throws<DomainValidationException>(() => Order.Create(
            new Address("A", "B"), new Address("C", "D"), new CargoWeight(1),
            new DateOnly(2026, 8, 23), Guid.NewGuid(), Now));

        Assert.Equal("pickupDate", exception.Field);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100001)]
    public void CargoWeight_RejectsInvalidValue(decimal value) =>
        Assert.Throws<DomainValidationException>(() => new CargoWeight(value));
}
