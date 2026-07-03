using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_matches_CargoListingId_VesselId_Active",
                table: "matches");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "matches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "matches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "matches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledBy",
                table: "matches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "matches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedBy",
                table: "matches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "matches",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "matches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedBy",
                table: "matches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "matches",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "vessel_reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VesselId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VesselAvailabilityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CargoListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservedWeight = table.Column<double>(type: "float", nullable: false),
                    IsReleased = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vessel_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vessel_reservations_cargo_listings_CargoListingId",
                        column: x => x.CargoListingId,
                        principalTable: "cargo_listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vessel_reservations_matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vessel_reservations_vessel_availabilities_VesselAvailabilityId",
                        column: x => x.VesselAvailabilityId,
                        principalTable: "vessel_availabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vessel_reservations_vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matches_CargoListingId_Approved",
                table: "matches",
                column: "CargoListingId",
                unique: true,
                filter: "[Status] = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "IX_matches_CargoListingId_VesselId_Active",
                table: "matches",
                columns: new[] { "CargoListingId", "VesselId" },
                unique: true,
                filter: "[Status] IN ('Proposed', 'PendingApproval', 'Approved')");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_reservations_CargoListingId",
                table: "vessel_reservations",
                column: "CargoListingId");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_reservations_MatchId",
                table: "vessel_reservations",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vessel_reservations_VesselAvailabilityId",
                table: "vessel_reservations",
                column: "VesselAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_vessel_reservations_VesselId_IsReleased",
                table: "vessel_reservations",
                columns: new[] { "VesselId", "IsReleased" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vessel_reservations");

            migrationBuilder.DropIndex(
                name: "IX_matches_CargoListingId_Approved",
                table: "matches");

            migrationBuilder.DropIndex(
                name: "IX_matches_CargoListingId_VesselId_Active",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "RowVersion",
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
