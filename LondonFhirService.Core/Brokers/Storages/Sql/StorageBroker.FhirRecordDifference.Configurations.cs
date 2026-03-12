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
                .IsRequired();

            model
                .Property(fhirRecordDifference => fhirRecordDifference.SecondaryId)
                .IsRequired();

            model
                .Property(fhirRecordDifference => fhirRecordDifference.CorrelationId)
                .IsRequired();

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
        }
    }
}
