// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Microsoft.EntityFrameworkCore;
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
                .IsRequired();

            model
                .Property(fhirRecord => fhirRecord.JsonPayload)
                .IsRequired();

            model
                .Property(fhirRecord => fhirRecord.SourceName)
                .IsRequired();

            model
                .Property(fhirRecord => fhirRecord.IsPrimarySource)
                .IsRequired();

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
