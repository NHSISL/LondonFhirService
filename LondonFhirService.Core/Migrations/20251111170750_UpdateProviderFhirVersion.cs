using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProviderFhirVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FhirVersion",
                table: "Providers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UX_Provider_FhirVersion_PrimaryOnly",
                table: "Providers",
                column: "FhirVersion",
                unique: true,
                filter: "[IsPrimary] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Provider_FhirVersion_PrimaryOnly",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "FhirVersion",
                table: "Providers");
        }
    }
}
