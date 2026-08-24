// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void AddFhirRecordConfigurations(EntityTypeBuilder<FhirRecord> model)
        {
            model
                .ToTable("FhirRecords");

            model
                .Property(fhirRecord => fhirRecord.Id)
                .IsRequired();

            model
                .Property(fhirRecord => fhirRecord.CorrelationId)
                .IsRequired()
                .HasMaxLength(255);

            model
                .HasIndex(fhirRecord => fhirRecord.CorrelationId);

            model
                .Property(fhirRecord => fhirRecord.JsonPayload)
                .IsRequired();

            model
                .Property(fhirRecord => fhirRecord.SourceName)
                .IsRequired();

            model
                .HasIndex(fhirRecord => fhirRecord.SourceName);

            model
                .Property(fhirRecord => fhirRecord.IsPrimarySource)
                .IsRequired();

            model
                .HasIndex(fhirRecord => fhirRecord.IsPrimarySource);

            model
                .Property(fhirRecord => fhirRecord.IsProcessed)
                .IsRequired();

            model
                .HasIndex(fhirRecord => fhirRecord.IsProcessed);

            // Stamped by the database on insert, so it measures when the row became visible to
            // the compare queue rather than when a request thread built it.
            //
            // Never written again. ValueGeneratedOnAdd alone still emits the column in UPDATE
            // statements, so an update carrying a whole entity - the management host's PUT, say -
            // would overwrite the stamp with whatever the caller sent, and a caller that simply
            // omitted it would write default(DateTimeOffset) and make the row instantly eligible,
            // collapsing the compare buffer it exists to enforce.
            model
                .Property(fhirRecord => fhirRecord.InsertedDate)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                .ValueGeneratedOnAdd()
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // The compare queue's claim query: Status and IsPrimarySource filter it, InsertedDate
            // ranges it, CreatedDate orders it. Status was previously in no index at all, so the
            // claim ran as a clustered-index scan on a table that grows by a row per provider per
            // request - once per record claimed, which made draining a backlog quadratic.
            model
                .HasIndex(fhirRecord => new
                {
                    fhirRecord.Status,
                    fhirRecord.IsPrimarySource,
                    fhirRecord.InsertedDate,
                    fhirRecord.CreatedDate
                });

            model
                .Property(fhirRecord => fhirRecord.CreatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(fhirRecord => fhirRecord.CreatedDate)
                .IsRequired();

            model
                .Property(fhirRecord => fhirRecord.UpdatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(fhirRecord => fhirRecord.UpdatedDate)
                .IsRequired();
        }
    }
}
