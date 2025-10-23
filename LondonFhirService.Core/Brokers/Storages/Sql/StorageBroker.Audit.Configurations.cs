// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void AddAuditConfigurations(EntityTypeBuilder<Audit> model)
        {
            model
                .ToTable("Audits");

            model
                .Property(audit => audit.Id)
                .IsRequired();

            model
                .Property(audit => audit.CorrelationId)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(audit => audit.AuditType)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(audit => audit.LogLevel)
                .HasMaxLength(255)
                .IsRequired(false);

            model
                .Property(audit => audit.FileName)
                .HasMaxLength(1000)
                .IsRequired(false);

            model
                .Property(audit => audit.Title)
                .IsRequired(false);

            model
                .Property(audit => audit.Message)
                .IsRequired(false);

            model
                .Property(audit => audit.CreatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(audit => audit.CreatedDate)
                .IsRequired();

            model
                .Property(audit => audit.UpdatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(audit => audit.UpdatedDate)
                .IsRequired();

            model
                .HasIndex(audit => audit.CorrelationId);

            model
                .HasIndex(audit => audit.AuditType);

            model
                .HasIndex(audit => audit.LogLevel);
        }
    }
}
