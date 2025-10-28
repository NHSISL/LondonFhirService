// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void AddProviderConfigurations(EntityTypeBuilder<Provider> model)
        {
            model
                .ToTable("Providers");

            model
                .Property(provider => provider.Id)
                .IsRequired();

            model
                .Property(provider => provider.Name)
                .HasMaxLength(500)
                .IsRequired();

            model
                .HasIndex(provider => provider.Name)
                .IsUnique();

            model
                .Property(provider => provider.IsActive)
                .IsRequired();

            model
                .Property(provider => provider.ActiveFrom)
                .IsRequired();

            model
                .Property(provider => provider.ActiveTo)
                .IsRequired();

            model
                .Property(provider => provider.IsForComparisonOnly)
                .IsRequired();

            model
                .Property(provider => provider.CreatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(provider => provider.CreatedDate)
                .IsRequired();

            model
                .Property(provider => provider.UpdatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(provider => provider.UpdatedDate)
                .IsRequired();
        }
    }
}
