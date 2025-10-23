// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.PdsDatas;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<PdsData> InsertPdsDataAsync(PdsData pdsData);
        ValueTask<IQueryable<PdsData>> SelectAllPdsDatasAsync();
        ValueTask<PdsData> SelectPdsDataByIdAsync(Guid pdsDataId);
        ValueTask<PdsData> UpdatePdsDataAsync(PdsData pdsData);
        ValueTask<PdsData> DeletePdsDataAsync(PdsData pdsData);
    }
}
