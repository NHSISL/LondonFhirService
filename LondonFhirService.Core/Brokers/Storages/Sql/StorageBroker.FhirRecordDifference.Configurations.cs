// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void AddFhirRecordDifferenceConfigurations(EntityTypeBuilder<FhirRecordDifference> model)
        {
            model
                .ToTable("FhirRecordDifferences");

            model
                .Property(fhirRecordDifference => fhirRecordDifference.Id)
                .IsRequired();

            model
                .Property(fhirRecordDifference => fhirRecordDifference.PrimaryId)
                .IsRequired()
                .HasMaxLength(255);

            model
                .HasIndex(fhirRecord => fhirRecord.PrimaryId);

            model
                .Property(fhirRecordDifference => fhirRecordDifference.SecondaryId)
                .IsRequired()
                .HasMaxLength(255);

            model
                .HasIndex(fhirRecord => fhirRecord.SecondaryId);

            model
                .Property(fhirRecordDifference => fhirRecordDifference.CorrelationId)
                .IsRequired()
                .HasMaxLength(255);

            model
                .HasIndex(fhirRecord => fhirRecord.CorrelationId);

            model
                .Property(fhirRecordDifference => fhirRecordDifference.DiffJson)
                .IsRequired();

            model
                .Property(fhirRecordDifference => fhirRecordDifference.CreatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(fhirRecordDifference => fhirRecordDifference.CreatedDate)
                .IsRequired();

            model
                .Property(fhirRecordDifference => fhirRecordDifference.UpdatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(fhirRecordDifference => fhirRecordDifference.UpdatedDate)
                .IsRequired();

            model
                .HasIndex(fhirRecord => fhirRecord.IsResolved);

            // Mapped so OData can $expand them and EF can translate that into a join. The foreign
            // key columns already exist, so this adds no column and moves no data.
            //
            // Neither side cascades, and neither is enforced by the database - see the migration,
            // where both AddForeignKey calls are removed by hand. SQL Server allows one cascade
            // path between a pair of tables and these are two paths to the same one, so a cascade
            // on both is rejected outright with error 1785, "may cause cycles or multiple cascade
            // paths". Rather than cascade on one side and leave the other to fail a delete, the
            // relationship stays a model concern: EF needs it to translate an expand, and does not
            // need the server to police it.
            //
            // Nothing needs the server to enforce it either way: cleanup deletes the differences
            // for a correlation id first and the records after, so the ordering a constraint would
            // impose is already the order the code uses.
            //
            // Leaving the constraints out also keeps difference rows insertable on their own,
            // which is how they are written today and how the tests build them, and makes the
            // migration safe against a table whose rows may already reference a FhirRecord that
            // retention has since removed.
            // IsRequired(false) on both, and it is the whole point rather than a detail. The
            // foreign key properties are non-nullable Guids, from which EF infers a required
            // relationship - and a required reference navigation is projected with an INNER JOIN.
            // With no database constraint to guarantee the other side exists, that join silently
            // dropped any difference row whose FhirRecord had gone: the comparisons page uses
            // $expand=Secondary, so the row vanished from the list with no error, and because the
            // page's "is there more" check is a row count against the page size, the paging
            // stopped early and hid every older comparison behind it.
            //
            // Optional makes it a LEFT JOIN and the row survives with an empty source name, which
            // is what the list already renders as a dash. The columns stay NOT NULL - this changes
            // how EF reads the relationship, not the table.
            model
                .HasOne(fhirRecordDifference => fhirRecordDifference.Secondary)
                .WithMany()
                .HasForeignKey(fhirRecordDifference => fhirRecordDifference.SecondaryId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            model
                .HasOne(fhirRecordDifference => fhirRecordDifference.Primary)
                .WithMany()
                .HasForeignKey(fhirRecordDifference => fhirRecordDifference.PrimaryId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        }
    }
}
