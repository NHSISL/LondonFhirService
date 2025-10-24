using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Types;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <inheritdoc />
    public partial class OdsAndPds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Audits",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LogLevel",
                table: "Audits",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Audits",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Audits",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "Audits",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuditType",
                table: "Audits",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "OdsDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OdsHierarchy = table.Column<SqlHierarchyId>(type: "hierarchyid", nullable: true),
                    OrganisationCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    OrganisationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RelationshipWithParentStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RelationshipWithParentEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    HasChildren = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdsDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PdsDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NhsNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    OrgCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    OrganisationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelationshipWithOrganisationEffectiveFromDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RelationshipWithOrganisationEffectiveToDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdsDatas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AuditType",
                table: "Audits",
                column: "AuditType");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_CorrelationId",
                table: "Audits",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_LogLevel",
                table: "Audits",
                column: "LogLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OdsDatas");

            migrationBuilder.DropTable(
                name: "PdsDatas");

            migrationBuilder.DropIndex(
                name: "IX_Audits_AuditType",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Audits_CorrelationId",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Audits_LogLevel",
                table: "Audits");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Audits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "LogLevel",
                table: "Audits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Audits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Audits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "Audits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "AuditType",
                table: "Audits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
