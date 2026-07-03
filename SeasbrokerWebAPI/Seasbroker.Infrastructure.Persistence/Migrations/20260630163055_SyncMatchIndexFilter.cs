using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncMatchIndexFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_matches_CargoListingId_VesselId_Active",
                table: "matches");

            migrationBuilder.CreateIndex(
                name: "IX_matches_CargoListingId_VesselId_Active",
                table: "matches",
                columns: new[] { "CargoListingId", "VesselId" },
                unique: true,
                filter: "[Status] IN ('Proposed', 'PendingApproval', 'Approved')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_matches_CargoListingId_VesselId_Active",
                table: "matches");

            migrationBuilder.CreateIndex(
                name: "IX_matches_CargoListingId_VesselId_Active",
                table: "matches",
                columns: new[] { "CargoListingId", "VesselId" },
                unique: true,
                filter: "[Status] IN ('Proposed', 'PendingApproval', 'Approved')");
        }
    }
}
