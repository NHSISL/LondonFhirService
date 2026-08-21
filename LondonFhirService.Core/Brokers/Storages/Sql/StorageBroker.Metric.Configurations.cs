// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void AddMetricConfigurations(EntityTypeBuilder<Metric> model)
        {
            model
                .ToTable("Metrics");

            model
                .Property(metric => metric.Id)
                .IsRequired();

            model
                .Property(metric => metric.ParentId)
                .IsRequired(false);

            model
                .Property(metric => metric.CorrelationId)
                .IsRequired();

            model
                .Property(metric => metric.Method)
                .HasMaxLength(255)
                .IsRequired();

            // Persisted as text rather than the ordinal used elsewhere in this broker. This table
            // exists to be queried ad hoc for reporting, where "WHERE Type = 'Provider'" is
            // readable and an ordinal is not, and where an enum reorder would silently rewrite
            // the meaning of historic rows.
            model
                .Property(metric => metric.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            model
                .Property(metric => metric.Name)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(metric => metric.Target)
                .HasMaxLength(255)
                .IsRequired(false);

            model
                .Property(metric => metric.Started)
                .IsRequired();

            model
                .Property(metric => metric.Completed)
                .IsRequired();

            model
                .Property(metric => metric.DurationMs)
                .IsRequired();

            model
                .Property(metric => metric.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            model
                .Property(metric => metric.ErrorCode)
                .HasMaxLength(100)
                .IsRequired(false);

            model
                .Property(metric => metric.PayloadBytes)
                .IsRequired(false);

            model
                .Property(metric => metric.Consumer)
                .HasMaxLength(255)
                .IsRequired(false);

            model
                .Property(metric => metric.CreatedDate)
                .IsRequired();

            // Assembling every span of one request.
            model
                .HasIndex(metric => metric.CorrelationId);

            // Walking the span tree from a parent to its children.
            model
                .HasIndex(metric => metric.ParentId);

            // The retention sweep, which deletes by age.
            model
                .HasIndex(metric => metric.CreatedDate);

            // The main reporting slice: durations for one operation and span kind over a window.
            model
                .HasIndex(metric => new { metric.Method, metric.Type, metric.Started });
        }
    }
}
