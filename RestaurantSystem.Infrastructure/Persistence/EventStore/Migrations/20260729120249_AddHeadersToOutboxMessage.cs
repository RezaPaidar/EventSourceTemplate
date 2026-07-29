using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RestaurantSystem.Infrastructure.Persistence.EventStore.Migrations
{
    /// <inheritdoc />
    public partial class AddHeadersToOutboxMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "EventStore");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "EventStore",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ProcessingStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockId = table.Column<Guid>(type: "uuid", nullable: true),
                    Headers = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredEvents",
                schema: "EventStore",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OccurredOnUtc",
                schema: "EventStore",
                table: "OutboxMessages",
                column: "OccurredOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "EventStore",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_OccurredOnUtc",
                schema: "EventStore",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StoredEvents_AggregateId_Version",
                schema: "EventStore",
                table: "StoredEvents",
                columns: new[] { "AggregateId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredEvents_AggregateType",
                schema: "EventStore",
                table: "StoredEvents",
                column: "AggregateType");

            migrationBuilder.CreateIndex(
                name: "IX_StoredEvents_EventType",
                schema: "EventStore",
                table: "StoredEvents",
                column: "EventType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "EventStore");

            migrationBuilder.DropTable(
                name: "StoredEvents",
                schema: "EventStore");
        }
    }
}
