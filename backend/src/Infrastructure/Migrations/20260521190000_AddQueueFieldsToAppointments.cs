using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueFieldsToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalTimestamp",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueuePosition",
                table: "Appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_QueuePosition_SlotId",
                table: "Appointments",
                columns: new[] { "QueuePosition", "SlotId" },
                filter: "\"QueuePosition\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_QueuePosition_SlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ArrivalTimestamp",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "QueuePosition",
                table: "Appointments");
        }
    }
}
