// Migrations/20251113_CreateAuditEventsTable.cs
using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectsPlus.Migrations
{
    public partial class CreateAuditEventsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                              .Annotation("SqlServer:Identity", "1, 1"),
                    event_type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    actor_id = table.Column<long>(type: "bigint", nullable: true),
                    target_id = table.Column<long>(type: "bigint", nullable: true),
                    target_type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    system_type_id = table.Column<long>(type: "bigint", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    payload_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    metadata_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_event_type",
                table: "audit_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_actor_id",
                table: "audit_events",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_correlation_id",
                table: "audit_events",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_occurred_at",
                table: "audit_events",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_target_id",
                table: "audit_events",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_actor_id_occurred_at",
                table: "audit_events",
                columns: new[] { "actor_id", "occurred_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "audit_events");
        }
    }
}
