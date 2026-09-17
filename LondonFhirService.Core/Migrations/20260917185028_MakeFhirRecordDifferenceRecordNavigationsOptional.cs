using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <summary>
    /// Empty on purpose, and generated empty rather than hand-emptied.
    ///
    /// The Primary and Secondary navigations were inferred as required, because their foreign key
    /// properties are non-nullable Guids - and EF projects a required reference navigation with an
    /// INNER JOIN. With no database constraint guaranteeing the other side exists, the comparisons
    /// page's $expand=Secondary silently dropped every difference row whose FhirRecord had gone,
    /// and truncated the paging behind it.
    ///
    /// Marking them optional changes how EF reads the relationship, not the table: the columns
    /// stay NOT NULL and no constraint is added or removed, so there is nothing for this migration
    /// to do. It exists to keep the model snapshot and the migration history agreeing with each
    /// other.
    /// </summary>
    public partial class MakeFhirRecordDifferenceRecordNavigationsOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Model-only. See the summary above.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Model-only. See the summary above.
        }
    }
}
