using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddFhirRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FhirRecordDifferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecondaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiffJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiffCount = table.Column<int>(type: "int", nullable: false),
                    AcceptablceDiffCount = table.Column<int>(type: "int", nullable: false),
                    ComparedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FhirRecordDifferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FhirRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JsonPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimarySource = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FhirRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FhirRecordDifferences");

            migrationBuilder.DropTable(
                name: "FhirRecords");
        }
    }
}
