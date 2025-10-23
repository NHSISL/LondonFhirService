// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using Microsoft.EntityFrameworkCore;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        public DbSet<OdsData> OdsDatas { get; set; }

        public async ValueTask<OdsData> InsertOdsDataAsync(OdsData odsData) =>
            await InsertAsync(odsData);

        public async ValueTask<IQueryable<OdsData>> SelectAllOdsDatasAsync() =>
            await SelectAllAsync<OdsData>();

        public async ValueTask<OdsData> SelectOdsDataByIdAsync(Guid odsDataId) =>
            await SelectAsync<OdsData>(odsDataId);
        public async ValueTask<OdsData> UpdateOdsDataAsync(OdsData odsData) =>
            await UpdateAsync(odsData);

        public async ValueTask<OdsData> DeleteOdsDataAsync(OdsData odsData) =>
            await DeleteAsync(odsData);
    }
}