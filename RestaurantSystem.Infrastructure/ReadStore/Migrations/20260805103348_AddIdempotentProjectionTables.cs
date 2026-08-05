using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSystem.Infrastructure.ReadStore.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotentProjectionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_projection_checkpoints",
                schema: "ReadStore",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LastProcessedVersion = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_projection_checkpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order_projection_processed_events",
                schema: "ReadStore",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventVersion = table.Column<long>(type: "bigint", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_projection_processed_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_projection_checkpoints_ProjectorName",
                schema: "ReadStore",
                table: "order_projection_checkpoints",
                column: "ProjectorName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_projection_processed_events_ProjectorName_EventId",
                schema: "ReadStore",
                table: "order_projection_processed_events",
                columns: new[] { "ProjectorName", "EventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_projection_checkpoints",
                schema: "ReadStore");

            migrationBuilder.DropTable(
                name: "order_projection_processed_events",
                schema: "ReadStore");
        }
    }
}
