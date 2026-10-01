using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTrackingNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                table: "requested_quotes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Give every existing request its own tracking number before the unique index goes on.
            // NEWID() is evaluated per row; the loop re-rolls the (very unlikely) duplicates.
            migrationBuilder.Sql(@"
UPDATE [requested_quotes]
SET [TrackingNumber] = 'SB-' + UPPER(LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 8))
WHERE [TrackingNumber] = '';

WHILE EXISTS (SELECT 1 FROM [requested_quotes] GROUP BY [TrackingNumber] HAVING COUNT(*) > 1)
BEGIN
    UPDATE q
    SET [TrackingNumber] = 'SB-' + UPPER(LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 8))
    FROM [requested_quotes] q
    WHERE q.[TrackingNumber] IN (SELECT [TrackingNumber] FROM [requested_quotes] GROUP BY [TrackingNumber] HAVING COUNT(*) > 1);
END");

            migrationBuilder.CreateIndex(
                name: "IX_requested_quotes_TrackingNumber",
                table: "requested_quotes",
                column: "TrackingNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_requested_quotes_TrackingNumber",
                table: "requested_quotes");

            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                table: "requested_quotes");
        }
    }
}
