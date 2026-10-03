using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselReeferPlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReeferPlugs",
                table: "vessels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Vessels already added to the fleet from a Ship Brokerage request keep what the customer
            // ticked on the form (Container Ship -> Reefer Plugs). Every other vessel stays false.
            migrationBuilder.Sql(@"
UPDATE v SET v.[ReeferPlugs] = 1
FROM [vessels] v
JOIN [form_submissions] s ON s.[RequestedQuoteId] = v.[RequestedQuoteId]
JOIN [form_submission_values] fv ON fv.[FormSubmissionId] = s.[Id]
WHERE v.[RequestedQuoteId] IS NOT NULL
  AND fv.[FieldKey] = 'contReeferPlugs'
  AND LOWER(fv.[ValueText]) = 'true';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReeferPlugs",
                table: "vessels");
        }
    }
}
