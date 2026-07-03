using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCargoListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cargo_listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedQuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CargoType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    Dimensions = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeparturePort = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalPort = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargo_listings", x => x.Id);
                    table.CheckConstraint("CK_cargo_listings_DateRange", "[DepartureTime] < [ArrivalTime]");
                    table.CheckConstraint("CK_cargo_listings_Priority", "[Priority] >= 1 AND [Priority] <= 5");
                    table.CheckConstraint("CK_cargo_listings_Weight", "[Weight] > 0");
                    table.ForeignKey(
                        name: "FK_cargo_listings_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cargo_listings_requested_quotes_RequestedQuoteId",
                        column: x => x.RequestedQuoteId,
                        principalTable: "requested_quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cargo_listings_CustomerId",
                table: "cargo_listings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_listings_Dates",
                table: "cargo_listings",
                columns: new[] { "DepartureTime", "ArrivalTime" });

            migrationBuilder.CreateIndex(
                name: "IX_cargo_listings_Ports",
                table: "cargo_listings",
                columns: new[] { "DeparturePort", "ArrivalPort" });

            migrationBuilder.CreateIndex(
                name: "IX_cargo_listings_ReferenceNumber",
                table: "cargo_listings",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cargo_listings_RequestedQuoteId",
                table: "cargo_listings",
                column: "RequestedQuoteId",
                unique: true,
                filter: "[RequestedQuoteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_listings_Status",
                table: "cargo_listings",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cargo_listings");
        }
    }
}
