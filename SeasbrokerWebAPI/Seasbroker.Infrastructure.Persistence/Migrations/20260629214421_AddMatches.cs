using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CargoListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VesselId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatchReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ScoreBreakdown = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                    table.CheckConstraint("CK_matches_Score", "[Score] >= 0 AND [Score] <= 100");
                    table.ForeignKey(
                        name: "FK_matches_cargo_listings_CargoListingId",
                        column: x => x.CargoListingId,
                        principalTable: "cargo_listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matches_chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_matches_vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "matching_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Criterion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Configuration = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matching_rules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matches_CargoListingId_Status",
                table: "matches",
                columns: new[] { "CargoListingId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_matches_CargoListingId_VesselId_Active",
                table: "matches",
                columns: new[] { "CargoListingId", "VesselId" },
                unique: true,
                filter: "[Status] IN ('Proposed', 'PendingApproval', 'Approved')");

            migrationBuilder.CreateIndex(
                name: "IX_matches_ChatId",
                table: "matches",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_Status_ExpiresAt",
                table: "matches",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_matches_VesselId_Status",
                table: "matches",
                columns: new[] { "VesselId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_matching_rules_Criterion",
                table: "matching_rules",
                column: "Criterion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matches");

            migrationBuilder.DropTable(
                name: "matching_rules");
        }
    }
}
