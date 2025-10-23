// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.OdsDatas;

namespace LondonFhirService.Core.Services.Foundations.OdsDatas
{
    public interface IOdsDataService
    {
        ValueTask<OdsData> AddOdsDataAsync(OdsData odsData);
        ValueTask<IQueryable<OdsData>> RetrieveAllOdsDatasAsync();
        ValueTask<OdsData> RetrieveOdsDataByIdAsync(Guid odsDataId);
        ValueTask<OdsData> ModifyOdsDataAsync(OdsData odsData);
        ValueTask<OdsData> RemoveOdsDataByIdAsync(Guid odsDataId);
        ValueTask<List<OdsData>> RetrieveChildrenByParentId(Guid odsDataId);
        ValueTask<List<OdsData>> RetrieveAllDecendentsByParentId(Guid odsDataId);
        ValueTask<List<OdsData>> RetrieveAllAncestorsByChildId(Guid odsDataId);
    }
}