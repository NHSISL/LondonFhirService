// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.OdsDatas;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void AddOdsDataConfigurations(EntityTypeBuilder<OdsData> builder)
        {
            builder.Property(OdsData => OdsData.OrganisationCode)
                .HasMaxLength(15)
                .IsRequired();

            builder.Property(OdsData => OdsData.OrganisationName)
                .HasMaxLength(255);
        }
    }
}