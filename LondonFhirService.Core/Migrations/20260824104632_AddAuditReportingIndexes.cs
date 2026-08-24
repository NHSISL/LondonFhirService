using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditReportingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Audits_AuditType",
                table: "Audits");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Audits",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AuditType_CreatedDate",
                table: "Audits",
                columns: new[] { "AuditType", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Audits_CreatedDate",
                table: "Audits",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_Title_CreatedDate",
                table: "Audits",
                columns: new[] { "Title", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Audits_AuditType_CreatedDate",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Audits_CreatedDate",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Audits_Title_CreatedDate",
                table: "Audits");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Audits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AuditType",
                table: "Audits",
                column: "AuditType");
        }
    }
}
