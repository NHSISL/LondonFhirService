// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.PdsDatas;

namespace LondonFhirService.Core.Services.Foundations.PdsDatas
{
    public partial class PdsDataService : IPdsDataService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public PdsDataService(
            IStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<PdsData> AddPdsDataAsync(PdsData pdsData) =>
            TryCatch(async () =>
            {
                ValidatePdsDataOnAdd(pdsData);

                return await this.storageBroker.InsertPdsDataAsync(pdsData);
            });

        public ValueTask<IQueryable<PdsData>> RetrieveAllPdsDatasAsync() =>
            TryCatch(this.storageBroker.SelectAllPdsDatasAsync);

        public ValueTask<PdsData> RetrievePdsDataByIdAsync(Guid pdsDataId) =>
            TryCatch(async () =>
            {
                ValidatePdsDataId(pdsDataId);

                PdsData maybePdsData = await this.storageBroker
                    .SelectPdsDataByIdAsync(pdsDataId);

                ValidateStoragePdsData(maybePdsData, pdsDataId);

                return maybePdsData;
            });

        public ValueTask<PdsData> ModifyPdsDataAsync(PdsData pdsData) =>
            TryCatch(async () =>
            {
                ValidatePdsDataOnModify(pdsData);

                PdsData maybePdsData =
                    await this.storageBroker.SelectPdsDataByIdAsync(pdsData.Id);

                ValidateStoragePdsData(maybePdsData, pdsData.Id);
                ValidateAgainstStoragePdsDataOnModify(inputPdsData: pdsData, storagePdsData: maybePdsData);

                return await this.storageBroker.UpdatePdsDataAsync(pdsData);
            });

        public ValueTask<PdsData> RemovePdsDataByIdAsync(Guid pdsDataId) =>
            TryCatch(async () =>
            {
                ValidatePdsDataId(pdsDataId);

                PdsData maybePdsData = await this.storageBroker
                    .SelectPdsDataByIdAsync(pdsDataId);

                ValidateStoragePdsData(maybePdsData, pdsDataId);

                return await this.storageBroker.DeletePdsDataAsync(maybePdsData);
            });

        public ValueTask<bool> OrganisationsHaveAccessToThisPatient(
            string nhsNumber,
            List<string> organisationCodes) =>
            TryCatch(async () =>
            {
                ValidateOnOrganisationsHaveAccessToThisPatient(nhsNumber, organisationCodes);

                var query = await this.storageBroker.SelectAllPdsDatasAsync();
                DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                bool hasAccess = query.Any(
                    pdsData => pdsData.NhsNumber == nhsNumber
                    && organisationCodes.Contains(pdsData.OrgCode)
                    && (pdsData.RelationshipWithOrganisationEffectiveFromDate == null
                        || pdsData.RelationshipWithOrganisationEffectiveFromDate <= currentDateTime)
                    && (pdsData.RelationshipWithOrganisationEffectiveToDate == null ||
                        pdsData.RelationshipWithOrganisationEffectiveToDate > currentDateTime));

                return hasAccess;
            });
    }
}