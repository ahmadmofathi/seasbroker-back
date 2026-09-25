using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselRequestedQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RequestedQuoteId",
                table: "vessels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vessels_RequestedQuoteId",
                table: "vessels",
                column: "RequestedQuoteId",
                unique: true,
                filter: "[RequestedQuoteId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_vessels_requested_quotes_RequestedQuoteId",
                table: "vessels",
                column: "RequestedQuoteId",
                principalTable: "requested_quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vessels_requested_quotes_RequestedQuoteId",
                table: "vessels");

            migrationBuilder.DropIndex(
                name: "IX_vessels_RequestedQuoteId",
                table: "vessels");

            migrationBuilder.DropColumn(
                name: "RequestedQuoteId",
                table: "vessels");
        }
    }
}
