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

            // Bounded so it can be an index key - nvarchar(max) cannot be one. Every title this
            // codebase writes is a short generated phrase; 500 leaves generous headroom.
            model
                .Property(audit => audit.Title)
                .HasMaxLength(500)
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

            // Assembling every entry of one request.
            model
                .HasIndex(audit => audit.CorrelationId);

            // Type-led reporting over a window; also serves AuditType-only filters as a prefix.
            model
                .HasIndex(audit => new { audit.AuditType, audit.CreatedDate });

            model
                .HasIndex(audit => audit.LogLevel);

            // Title-led slices ("Access Forbidden") over a window.
            model
                .HasIndex(audit => new { audit.Title, audit.CreatedDate });

            // Time windows and the retention sweep.
            model
                .HasIndex(audit => audit.CreatedDate);
        }
    }
}
