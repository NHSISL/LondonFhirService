using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddFhirRecordIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AcceptablceDiffCount",
                table: "FhirRecordDifferences",
                newName: "AcceptableDiffCount");

            migrationBuilder.AlterColumn<string>(
                name: "SourceName",
                table: "FhirRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "FhirRecords",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "FhirRecordDifferences",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ComparedAt",
                table: "FhirRecordDifferences",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_FhirRecords_CorrelationId",
                table: "FhirRecords",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FhirRecords_IsPrimarySource",
                table: "FhirRecords",
                column: "IsPrimarySource");

            migrationBuilder.CreateIndex(
                name: "IX_FhirRecords_SourceName",
                table: "FhirRecords",
                column: "SourceName");

            migrationBuilder.CreateIndex(
                name: "IX_FhirRecordDifferences_CorrelationId",
                table: "FhirRecordDifferences",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FhirRecordDifferences_IsResolved",
                table: "FhirRecordDifferences",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_FhirRecordDifferences_PrimaryId",
                table: "FhirRecordDifferences",
                column: "PrimaryId");

            migrationBuilder.CreateIndex(
                name: "IX_FhirRecordDifferences_SecondaryId",
                table: "FhirRecordDifferences",
                column: "SecondaryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FhirRecords_CorrelationId",
                table: "FhirRecords");

            migrationBuilder.DropIndex(
                name: "IX_FhirRecords_IsPrimarySource",
                table: "FhirRecords");

            migrationBuilder.DropIndex(
                name: "IX_FhirRecords_SourceName",
                table: "FhirRecords");

            migrationBuilder.DropIndex(
                name: "IX_FhirRecordDifferences_CorrelationId",
                table: "FhirRecordDifferences");

            migrationBuilder.DropIndex(
                name: "IX_FhirRecordDifferences_IsResolved",
                table: "FhirRecordDifferences");

            migrationBuilder.DropIndex(
                name: "IX_FhirRecordDifferences_PrimaryId",
                table: "FhirRecordDifferences");

            migrationBuilder.DropIndex(
                name: "IX_FhirRecordDifferences_SecondaryId",
                table: "FhirRecordDifferences");

            migrationBuilder.RenameColumn(
                name: "AcceptableDiffCount",
                table: "FhirRecordDifferences",
                newName: "AcceptablceDiffCount");

            migrationBuilder.AlterColumn<string>(
                name: "SourceName",
                table: "FhirRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "FhirRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<Guid>(
                name: "CorrelationId",
                table: "FhirRecordDifferences",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ComparedAt",
                table: "FhirRecordDifferences",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");
        }
    }
}
