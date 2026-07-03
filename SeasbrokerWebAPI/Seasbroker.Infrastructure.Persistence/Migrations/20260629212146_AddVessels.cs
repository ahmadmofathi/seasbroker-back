using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVessels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vessels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImoNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VesselType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Dwt = table.Column<double>(type: "float", nullable: false),
                    TeuCapacity = table.Column<int>(type: "int", nullable: true),
                    LengthOverall = table.Column<double>(type: "float", nullable: true),
                    Beam = table.Column<double>(type: "float", nullable: true),
                    Draft = table.Column<double>(type: "float", nullable: true),
                    CurrentPort = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FlagCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vessels", x => x.Id);
                    table.CheckConstraint("CK_vessels_Dwt", "[Dwt] > 0");
                    table.ForeignKey(
                        name: "FK_vessels_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vessel_availabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VesselId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvailableTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpenPort = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DestinationPort = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vessel_availabilities", x => x.Id);
                    table.CheckConstraint("CK_vessel_availabilities_DateRange", "[AvailableFrom] < [AvailableTo]");
                    table.ForeignKey(
                        name: "FK_vessel_availabilities_vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vessel_availabilities_OpenPort_Dates",
                table: "vessel_availabilities",
                columns: new[] { "OpenPort", "AvailableFrom", "AvailableTo" });

            migrationBuilder.CreateIndex(
                name: "IX_vessel_availabilities_VesselId_IsActive",
                table: "vessel_availabilities",
                columns: new[] { "VesselId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_vessels_CurrentPort",
                table: "vessels",
                column: "CurrentPort");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_CustomerId",
                table: "vessels",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_ImoNumber",
                table: "vessels",
                column: "ImoNumber",
                unique: true,
                filter: "[ImoNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_Status",
                table: "vessels",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_vessels_VesselType",
                table: "vessels",
                column: "VesselType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vessel_availabilities");

            migrationBuilder.DropTable(
                name: "vessels");
        }
    }
}
