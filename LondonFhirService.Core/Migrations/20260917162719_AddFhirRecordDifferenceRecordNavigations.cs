// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LondonFhirService.Core.Migrations
{
    /// <summary>
    /// Model only, by design. FhirRecordDifference gained Primary and Secondary navigations so
    /// OData can $expand them and EF can turn that into a join - the foreign key columns and their
    /// indexes were already there, so nothing about the table changes.
    ///
    /// Hand edited: EF generated an AddForeignKey for each navigation and both are removed.
    ///
    /// Neither could usefully cascade. SQL Server permits one cascade path between a pair of
    /// tables and these are two paths to the same one, so cascading both is rejected outright with
    /// error 1785. Cascading one would leave deletes failing on the side that did not, which is
    /// worse than not cascading at all.
    ///
    /// Nothing needs the server to enforce this. Cleanup removes the FhirRecordDifferences for a
    /// correlation id first and the FhirRecords after, so the ordering a constraint would impose
    /// is already the order the code uses. EF needs the relationship in the model to translate an
    /// expand; it does not need a constraint to do it.
    ///
    /// Leaving them out also keeps difference rows insertable on their own, which is how they are
    /// written today, and makes this safe against a table whose rows may already point at a
    /// FhirRecord that retention has removed.
    /// </summary>
    public partial class AddFhirRecordDifferenceRecordNavigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
