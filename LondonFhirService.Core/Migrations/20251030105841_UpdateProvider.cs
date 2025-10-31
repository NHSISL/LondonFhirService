using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Providers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "Providers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "System",
                table: "Providers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "System",
                table: "Providers");
        }
    }
}
