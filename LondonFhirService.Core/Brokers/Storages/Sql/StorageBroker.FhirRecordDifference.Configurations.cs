// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
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
        }
    }
}
