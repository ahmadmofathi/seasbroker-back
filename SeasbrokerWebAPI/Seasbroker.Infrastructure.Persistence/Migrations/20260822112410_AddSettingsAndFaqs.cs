using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsAndFaqs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "faqs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Heading = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Para = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faqs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_settings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "system_settings",
                columns: new[] { "Id", "Key", "Value", "Created", "Updated" },
                values: new object[,]
                {
                    { Guid.NewGuid(), "address", "Alexandria, Egypt", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "phone", "+20 102 3456 789", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "email", "info@seasbroker.com", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "facebook", "https://facebook.com", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "twitter", "https://twitter.com", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "linkedin", "https://linkedin.com", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "instagram", "https://instagram.com", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "privacy_policy", "<h3>Digital Shipping Solution Privacy Policy</h3><p>Your privacy is important to us. It is Seasbroker's policy to respect your privacy regarding any information we may collect from you across our website, and other sites we own and operate.</p><p>We only ask for personal information when we truly need it to provide a service to you. We collect it by fair and lawful means, with your knowledge and consent.</p>", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "terms_conditions", "<h3>Seasbroker Terms & Conditions</h3><p>Welcome to Seasbroker. By accessing this website we assume you accept these terms and conditions in full. Do not continue to use Seasbroker's website if you do not accept all of the terms and conditions stated on this page.</p><p>The following terminology applies to these Terms and Conditions: 'Client', 'You' and 'Your' refers to you, the person accessing this website.</p>", DateTime.UtcNow, DateTime.UtcNow }
                });

            migrationBuilder.InsertData(
                table: "faqs",
                columns: new[] { "Id", "Heading", "Para", "SortOrder", "Created", "Updated" },
                values: new object[,]
                {
                    { Guid.NewGuid(), "TRANSPORT & LOGISTIC SERVICES", "Solving your supply chain needs from end to end, taking the complexity out of container shipping. We are at the forefront of developing innovation.", 1, DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "SAFE AND FASTER LOGISTIC SERVICE NEAR YOU", "Our warehousing and shipping services are recognized nationwide for their reliability, safety, and affordability, reflecting our commitment.", 2, DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.NewGuid(), "DIGITAL SHIPPING SOLUTION", "We facilitate the delivery of cargoes and match them with appropriate vessel options dynamically, ensuring smooth and transparent operations.", 3, DateTime.UtcNow, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "faqs");

            migrationBuilder.DropTable(
                name: "system_settings");
        }
    }
}
