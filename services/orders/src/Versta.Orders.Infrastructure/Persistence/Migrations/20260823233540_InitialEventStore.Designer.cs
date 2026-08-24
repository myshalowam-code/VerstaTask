using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Versta.Orders.Infrastructure.Persistence;

#nullable disable

namespace Versta.Orders.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(EventStoreDbContext))]
    [Migration("20260823233540_InitialEventStore")]
    partial class InitialEventStore
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Versta.Orders.Infrastructure.Persistence.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTimeOffset>("OccurredAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("PublishedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("StreamId")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int>("StreamVersion")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PublishedAtUtc");

                    b.ToTable("order_outbox", (string)null);
                });

            modelBuilder.Entity("Versta.Orders.Infrastructure.Persistence.StoredEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTimeOffset>("OccurredAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("StreamId")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("StreamType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("StreamVersion")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("StreamId", "StreamVersion")
                        .IsUnique();

                    b.ToTable("order_events", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
