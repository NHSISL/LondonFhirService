// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.Consumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void AddConsumerConfigurations(EntityTypeBuilder<Consumer> model)
        {
            model
                .ToTable("Consumers");

            model
                .Property(consumer => consumer.Id)
                .IsRequired();

            model
                .Property(consumer => consumer.UserId)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(consumer => consumer.Name)
                .IsRequired();

            model
                .HasIndex(consumer => consumer.Name)
                .IsUnique();

            model
                .Property(consumer => consumer.ContactName)
                .HasMaxLength(255);

            model
                .Property(consumer => consumer.ContactEmail)
                .HasMaxLength(320);

            model
                .Property(consumer => consumer.ContactPhone)
                .HasMaxLength(20);

            model
                .Property(consumer => consumer.ActiveFrom)
                .IsRequired();

            model
                .Property(consumer => consumer.CreatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(consumer => consumer.CreatedDate)
                .IsRequired();

            model
                .Property(consumer => consumer.UpdatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(consumer => consumer.UpdatedDate)
                .IsRequired();
        }
    }
}
