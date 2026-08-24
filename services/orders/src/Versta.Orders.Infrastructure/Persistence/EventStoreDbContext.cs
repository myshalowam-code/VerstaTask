using Microsoft.EntityFrameworkCore;

namespace Versta.Orders.Infrastructure.Persistence;

public sealed class EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : DbContext(options)
{
    public DbSet<StoredEvent> Events => Set<StoredEvent>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<StoredEvent>();
        entity.ToTable("order_events");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.StreamId, x.StreamVersion }).IsUnique();
        entity.Property(x => x.StreamId).HasMaxLength(200).IsRequired();
        entity.Property(x => x.StreamType).HasMaxLength(100).IsRequired();
        entity.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();

        var outbox = modelBuilder.Entity<OutboxMessage>();
        outbox.ToTable("order_outbox");
        outbox.HasKey(x => x.Id);
        outbox.HasIndex(x => x.PublishedAtUtc);
        outbox.Property(x => x.StreamId).HasMaxLength(200).IsRequired();
        outbox.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        outbox.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
    }
}
