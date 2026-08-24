using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Versta.Orders.Infrastructure.Persistence.Migrations
{
    public partial class InitialEventStore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StreamId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StreamType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StreamVersion = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order_outbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StreamId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StreamVersion = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_outbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_events_StreamId_StreamVersion",
                table: "order_events",
                columns: new[] { "StreamId", "StreamVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_outbox_PublishedAtUtc",
                table: "order_outbox",
                column: "PublishedAtUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_events");

            migrationBuilder.DropTable(
                name: "order_outbox");
        }
    }
}
