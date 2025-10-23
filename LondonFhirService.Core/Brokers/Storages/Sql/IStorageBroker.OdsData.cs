// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.OdsDatas;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<OdsData> InsertOdsDataAsync(OdsData odsData);
        ValueTask<IQueryable<OdsData>> SelectAllOdsDatasAsync();
        ValueTask<OdsData> SelectOdsDataByIdAsync(Guid odsDataId);
        ValueTask<OdsData> UpdateOdsDataAsync(OdsData odsData);
        ValueTask<OdsData> DeleteOdsDataAsync(OdsData odsData);
    }
}
