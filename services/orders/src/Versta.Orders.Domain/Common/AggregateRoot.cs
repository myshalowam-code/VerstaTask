namespace Versta.Orders.Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _uncommittedEvents = [];

    public int Version { get; private set; }
    public IReadOnlyCollection<DomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    protected void Raise(DomainEvent domainEvent)
    {
        Apply(domainEvent);
        _uncommittedEvents.Add(domainEvent);
    }

    public void LoadFromHistory(IEnumerable<DomainEvent> history)
    {
        foreach (var domainEvent in history)
        {
            Apply(domainEvent);
            Version++;
        }
    }

    public void MarkChangesAsCommitted()
    {
        Version += _uncommittedEvents.Count;
        _uncommittedEvents.Clear();
    }

    protected abstract void Apply(DomainEvent domainEvent);
}
