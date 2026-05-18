using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- AvailabilitySlots: overlap prevention (AC-01, Edge Case) ---
            // Prevents two slots for the same provider with identical start/end boundaries.
            // Application-level validation enforces broader overlap checks; this index
            // acts as the database-level safety net for exact boundary duplicates.
            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_ProviderId_StartTime_EndTime",
                table: "AvailabilitySlots",
                columns: new[] { "ProviderId", "StartTime", "EndTime" },
                unique: true);

            // --- PreferredSlotQueues: FCFS ordering index (AC-03) ---
            // Supports efficient retrieval of waiting queue entries for a slot
            // ordered by RequestedAt to enforce first-come-first-served semantics.
            migrationBuilder.CreateIndex(
                name: "IX_PreferredSlotQueues_PreferredSlotId_RequestedAt",
                table: "PreferredSlotQueues",
                columns: new[] { "PreferredSlotId", "RequestedAt" });

            // --- Notifications: RetryCount default (AC-05) ---
            // Sets the database-level default for RetryCount to 0 so rows inserted
            // without an explicit value initialise correctly.
            migrationBuilder.Sql("""
                ALTER TABLE "Notifications"
                ALTER COLUMN "RetryCount" SET DEFAULT 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Notifications"
                ALTER COLUMN "RetryCount" DROP DEFAULT;
                """);

            migrationBuilder.DropIndex(
                name: "IX_PreferredSlotQueues_PreferredSlotId_RequestedAt",
                table: "PreferredSlotQueues");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_ProviderId_StartTime_EndTime",
                table: "AvailabilitySlots");
        }
    }
}
