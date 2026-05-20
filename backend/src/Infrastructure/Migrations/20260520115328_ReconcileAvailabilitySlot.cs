using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReconcileAvailabilitySlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PreferredSlotQueues_PreferredSlotId",
                table: "PreferredSlotQueues");

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "AvailabilitySlots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "AvailabilitySlots");

            migrationBuilder.CreateIndex(
                name: "IX_PreferredSlotQueues_PreferredSlotId",
                table: "PreferredSlotQueues",
                column: "PreferredSlotId");
        }
    }
}
