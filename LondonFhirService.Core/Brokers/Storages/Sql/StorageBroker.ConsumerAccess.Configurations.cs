// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void AddConsumerAccessConfigurations(EntityTypeBuilder<ConsumerAccess> model)
        {
            model
                .ToTable("ConsumerAccesses");

            model
                .Property(consumerAccess => consumerAccess.Id)
                .IsRequired();

            model
                .Property(consumerAccess => consumerAccess.ConsumerId)
                .IsRequired();

            model
                .Property(consumerAccess => consumerAccess.OrgCode)
                .IsRequired();

            model
                .Property(consumerAccess => consumerAccess.CreatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(consumerAccess => consumerAccess.CreatedDate)
                .IsRequired();

            model
                .Property(consumerAccess => consumerAccess.UpdatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(consumerAccess => consumerAccess.UpdatedDate)
                .IsRequired();

            model
               .HasOne(consumerAccess => consumerAccess.Consumer)
               .WithMany(consumer => consumer.ConsumerAccesses)
               .HasForeignKey(consumerAccess => consumerAccess.ConsumerId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
