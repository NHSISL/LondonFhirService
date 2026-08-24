using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddFhirRecordInsertedDateAndCompareQueueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InsertedDate",
                table: "FhirRecords",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            // The column default stamps every existing row with the moment this migration ran,
            // which would make the pending backlog look brand new and hold the compare queue off
            // it for the buffer period. CreatedDate is the closest record of when those rows
            // really landed, so the rows the queue can still pick up keep their place.
            //
            // Restricted to unprocessed rows on purpose. This runs inside the migration's
            // transaction, and both hosts migrate at startup, so an unbounded UPDATE over a table
            // that grows by a row per provider per request and is never purged could exceed the
            // provider's 30-second default command timeout, roll the transaction back, leave
            // __EFMigrationsHistory unstamped, and put the host into a crash loop that replays
            // the same statement on every restart. Processed rows are terminal - nothing reads
            // their InsertedDate - so backfilling them buys nothing and risks exactly that.
            migrationBuilder.Sql(
                "UPDATE [FhirRecords] SET [InsertedDate] = [CreatedDate] WHERE [IsProcessed] = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_FhirRecords_Status_IsPrimarySource_InsertedDate_CreatedDate",
                table: "FhirRecords",
                columns: new[] { "Status", "IsPrimarySource", "InsertedDate", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FhirRecords_Status_IsPrimarySource_InsertedDate_CreatedDate",
                table: "FhirRecords");

            migrationBuilder.DropColumn(
                name: "InsertedDate",
                table: "FhirRecords");
        }
    }
}
