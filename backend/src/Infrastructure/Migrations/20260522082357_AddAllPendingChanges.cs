using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "CalendarSyncs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFailedLoginAtUtc",
                table: "CalendarSyncs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntilUtc",
                table: "CalendarSyncs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalTimestamp",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsurancePolicyNumber",
                table: "Appointments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceProvider",
                table: "Appointments",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueuePosition",
                table: "Appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Appointments",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "StaffNotifications",
                columns: table => new
                {
                    StaffNotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Variant = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffNotifications", x => x.StaffNotificationId);
                    table.ForeignKey(
                        name: "FK_StaffNotifications_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_QueuePosition_SlotId",
                table: "Appointments",
                columns: new[] { "QueuePosition", "SlotId" },
                filter: "\"QueuePosition\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StaffNotifications_CreatedAt",
                table: "StaffNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StaffNotifications_StaffUserId",
                table: "StaffNotifications",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffNotifications_StaffUserId_IsRead",
                table: "StaffNotifications",
                columns: new[] { "StaffUserId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_QueuePosition_SlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "CalendarSyncs");

            migrationBuilder.DropColumn(
                name: "LastFailedLoginAtUtc",
                table: "CalendarSyncs");

            migrationBuilder.DropColumn(
                name: "LockedUntilUtc",
                table: "CalendarSyncs");

            migrationBuilder.DropColumn(
                name: "ArrivalTimestamp",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "InsurancePolicyNumber",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "InsuranceProvider",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "QueuePosition",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Appointments");
        }
    }
}
