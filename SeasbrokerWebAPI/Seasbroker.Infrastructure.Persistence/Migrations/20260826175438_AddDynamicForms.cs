using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seasbroker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "form_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "form_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_versions_form_definitions_FormDefinitionId",
                        column: x => x.FormDefinitionId,
                        principalTable: "form_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_sections_form_versions_FormVersionId",
                        column: x => x.FormVersionId,
                        principalTable: "form_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedQuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_submissions_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_form_submissions_form_versions_FormVersionId",
                        column: x => x.FormVersionId,
                        principalTable: "form_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_form_submissions_requested_quotes_RequestedQuoteId",
                        column: x => x.RequestedQuoteId,
                        principalTable: "requested_quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "form_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Placeholder = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HelpText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Required = table.Column<bool>(type: "bit", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsSystemField = table.Column<bool>(type: "bit", nullable: false),
                    SystemFieldKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ValidationJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ConditionCombinator = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_fields_form_sections_FormSectionId",
                        column: x => x.FormSectionId,
                        principalTable: "form_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_form_fields_form_versions_FormVersionId",
                        column: x => x.FormVersionId,
                        principalTable: "form_versions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "form_submission_files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_submission_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_submission_files_form_submissions_FormSubmissionId",
                        column: x => x.FormSubmissionId,
                        principalTable: "form_submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_submission_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValueText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_submission_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_submission_values_form_submissions_FormSubmissionId",
                        column: x => x.FormSubmissionId,
                        principalTable: "form_submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_field_conditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceFieldKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_field_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_field_conditions_form_fields_FormFieldId",
                        column: x => x.FormFieldId,
                        principalTable: "form_fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_field_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_field_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_form_field_options_form_fields_FormFieldId",
                        column: x => x.FormFieldId,
                        principalTable: "form_fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_form_definitions_Key",
                table: "form_definitions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_field_conditions_FormFieldId",
                table: "form_field_conditions",
                column: "FormFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_form_field_options_FormFieldId",
                table: "form_field_options",
                column: "FormFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_form_fields_FormSectionId",
                table: "form_fields",
                column: "FormSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_form_fields_FormVersionId_Key",
                table: "form_fields",
                columns: new[] { "FormVersionId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_sections_FormVersionId_Key",
                table: "form_sections",
                columns: new[] { "FormVersionId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_submission_files_FormSubmissionId",
                table: "form_submission_files",
                column: "FormSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_form_submission_values_FormSubmissionId_FieldKey",
                table: "form_submission_values",
                columns: new[] { "FormSubmissionId", "FieldKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_form_submissions_CustomerId",
                table: "form_submissions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_form_submissions_FormVersionId",
                table: "form_submissions",
                column: "FormVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_form_submissions_RequestedQuoteId",
                table: "form_submissions",
                column: "RequestedQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_form_versions_FormDefinitionId_Status",
                table: "form_versions",
                columns: new[] { "FormDefinitionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_form_versions_FormDefinitionId_VersionNumber",
                table: "form_versions",
                columns: new[] { "FormDefinitionId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "form_field_conditions");

            migrationBuilder.DropTable(
                name: "form_field_options");

            migrationBuilder.DropTable(
                name: "form_submission_files");

            migrationBuilder.DropTable(
                name: "form_submission_values");

            migrationBuilder.DropTable(
                name: "form_fields");

            migrationBuilder.DropTable(
                name: "form_submissions");

            migrationBuilder.DropTable(
                name: "form_sections");

            migrationBuilder.DropTable(
                name: "form_versions");

            migrationBuilder.DropTable(
                name: "form_definitions");
        }
    }
}
