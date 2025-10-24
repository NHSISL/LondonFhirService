// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.OdsDatas;

namespace LondonFhirService.Core.Services.Foundations.OdsDatas
{
    public partial class OdsDataService : IOdsDataService
    {
        private readonly IStorageBroker storageBroker;
        private readonly ILoggingBroker loggingBroker;

        public OdsDataService(IStorageBroker storageBroker, ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<OdsData> AddOdsDataAsync(OdsData odsData) =>
        TryCatch(async () =>
        {
            await ValidateOdsDataOnAddAsync(odsData);

            return await this.storageBroker.InsertOdsDataAsync(odsData);
        });

        public ValueTask<IQueryable<OdsData>> RetrieveAllOdsDatasAsync() =>
         TryCatch(this.storageBroker.SelectAllOdsDatasAsync);

        public ValueTask<OdsData> RetrieveOdsDataByIdAsync(Guid odsDataId) =>
        TryCatch(async () =>
        {
            ValidateOdsDataId(odsDataId);

            OdsData maybeOdsData = await this.storageBroker
                .SelectOdsDataByIdAsync(odsDataId);

            ValidateStorageOdsData(maybeOdsData, odsDataId);

            return maybeOdsData;
        });

        public ValueTask<OdsData> ModifyOdsDataAsync(OdsData odsData) =>
        TryCatch(async () =>
        {
            await ValidateOdsDataOnModifyAsync(odsData);

            OdsData maybeOdsData =
                await this.storageBroker.SelectOdsDataByIdAsync(odsData.Id);

            ValidateStorageOdsData(maybeOdsData, odsData.Id);
            ValidateAgainstStorageOdsDataOnModify(inputOdsData: odsData, storageOdsData: maybeOdsData);

            return await this.storageBroker.UpdateOdsDataAsync(odsData);
        });

        public ValueTask<OdsData> RemoveOdsDataByIdAsync(Guid odsDataId) =>
        TryCatch(async () =>
        {
            ValidateOdsDataId(odsDataId);

            OdsData maybeOdsData = await this.storageBroker
                .SelectOdsDataByIdAsync(odsDataId);

            ValidateStorageOdsData(maybeOdsData, odsDataId);

            return await this.storageBroker.DeleteOdsDataAsync(maybeOdsData);
        });

        public ValueTask<List<OdsData>> RetrieveChildrenByParentId(Guid odsDataParentId) =>
        TryCatch(async () =>
        {
            ValidateOdsDataId(odsDataParentId);

            OdsData parentRecord = await this.storageBroker
                .SelectOdsDataByIdAsync(odsDataParentId);

            ValidateStorageOdsData(parentRecord, odsDataParentId);

            IQueryable<OdsData> query = await this.storageBroker.SelectAllOdsDatasAsync();
            query = query.Where(ods => ods.OdsHierarchy.GetAncestor(1) == parentRecord.OdsHierarchy);
            List<OdsData> children = query.ToList();

            return children;
        });

        public ValueTask<List<OdsData>> RetrieveAllDescendantsByParentId(Guid odsDataParentId) =>
        TryCatch(async () =>
        {
            ValidateOdsDataId(odsDataParentId);

            OdsData parentRecord = await this.storageBroker
                .SelectOdsDataByIdAsync(odsDataParentId);

            ValidateStorageOdsData(parentRecord, odsDataParentId);
            IQueryable<OdsData> query = await this.storageBroker.SelectAllOdsDatasAsync();
            query = query.Where(ods => ods.OdsHierarchy.IsDescendantOf(parentRecord.OdsHierarchy));
            List<OdsData> descendants = query.ToList();

            return descendants;
        });

        public ValueTask<List<OdsData>> RetrieveAllAncestorsByChildId(Guid odsDataChildId) =>
        TryCatch(async () =>
        {
            ValidateOdsDataId(odsDataChildId);

            OdsData childRecord = await this.storageBroker
                 .SelectOdsDataByIdAsync(odsDataChildId);

            ValidateStorageOdsData(childRecord, odsDataChildId);
            OdsData currentNode = childRecord;
            List<OdsData> ancestors = new List<OdsData>();
            ancestors.Add(currentNode);
            IQueryable<OdsData> odsDatas = await this.storageBroker.SelectAllOdsDatasAsync();

            while (currentNode.OdsHierarchy.GetLevel() >= 1)
            {
                int level = currentNode.OdsHierarchy.GetLevel();

                OdsData ancestor = odsDatas.FirstOrDefault(odsData =>
                    odsData.OdsHierarchy == currentNode.OdsHierarchy.GetAncestor(1));

                if (ancestor is not null)
                {
                    ancestors.Add(ancestor);
                    currentNode = ancestor;
                }
                else
                {
                    break;
                }
            }

            return ancestors;
        });
    }
}