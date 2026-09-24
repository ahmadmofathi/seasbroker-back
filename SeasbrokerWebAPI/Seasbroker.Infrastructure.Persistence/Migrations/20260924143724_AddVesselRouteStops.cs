using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselRouteStops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RouteStops",
                table: "vessel_availabilities",
                type: "nvarchar(max)",
                nullable: true);

            // Existing windows have no route yet - store an empty list rather than NULL.
            migrationBuilder.Sql("UPDATE [vessel_availabilities] SET [RouteStops] = N'[]' WHERE [RouteStops] IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteStops",
                table: "vessel_availabilities");
        }
    }
}
