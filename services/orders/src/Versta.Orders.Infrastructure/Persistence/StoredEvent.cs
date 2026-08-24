namespace Versta.Orders.Infrastructure.Persistence;

public sealed class StoredEvent
{
    public Guid Id { get; set; }
    public string StreamId { get; set; } = string.Empty;
    public string StreamType { get; set; } = string.Empty;
    public int StreamVersion { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
}
