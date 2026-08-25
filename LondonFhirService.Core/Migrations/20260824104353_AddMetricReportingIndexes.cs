using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricReportingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Metrics_Completed",
                table: "Metrics",
                column: "Completed");

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_Consumer_Started",
                table: "Metrics",
                columns: new[] { "Consumer", "Started" });

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_Name_Started",
                table: "Metrics",
                columns: new[] { "Name", "Started" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Metrics_Completed",
                table: "Metrics");

            migrationBuilder.DropIndex(
                name: "IX_Metrics_Consumer_Started",
                table: "Metrics");

            migrationBuilder.DropIndex(
                name: "IX_Metrics_Name_Started",
                table: "Metrics");
        }
    }
}
