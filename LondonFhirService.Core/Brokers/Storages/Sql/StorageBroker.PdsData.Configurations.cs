// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Models.Foundations.PdsDatas;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void AddPdsDataConfigurations(EntityTypeBuilder<PdsData> builder)
        {
            builder.HasKey(pdsData => pdsData.Id);

            builder.Property(pdsData => pdsData.Id)
                .IsRequired();

            builder.Property(pdsData => pdsData.NhsNumber)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(pdsData => pdsData.OrgCode)
                .HasMaxLength(15)
                .IsRequired();
        }
    }
}
