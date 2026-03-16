// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;

namespace LondonFhirService.Core.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceService : IFhirRecordDifferenceService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ISecurityBroker securityBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public FhirRecordDifferenceService(
            IStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            ISecurityBroker securityBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.securityBroker = securityBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<FhirRecordDifference> AddFhirRecordDifferenceAsync(FhirRecordDifference fhirRecordDifference) =>
            TryCatch(async () =>
            {
                fhirRecordDifference = await this.securityAuditBroker.ApplyAddAuditValuesAsync(fhirRecordDifference);
                await ValidateFhirRecordDifferenceOnAdd(fhirRecordDifference);

                return await this.storageBroker.InsertFhirRecordDifferenceAsync(fhirRecordDifference);
            });

        public ValueTask<IQueryable<FhirRecordDifference>> RetrieveAllFhirRecordDifferencesAsync() =>
            TryCatch(async () => await this.storageBroker.SelectAllFhirRecordDifferencesAsync());

        public ValueTask<FhirRecordDifference> RetrieveFhirRecordDifferenceByIdAsync(Guid fhirRecordDifferenceId) =>
            TryCatch(async () =>
            {
                ValidateFhirRecordDifferenceId(fhirRecordDifferenceId);

                FhirRecordDifference maybeFhirRecordDifference = await this.storageBroker
                    .SelectFhirRecordDifferenceByIdAsync(fhirRecordDifferenceId);

                ValidateStorageFhirRecordDifference(maybeFhirRecordDifference, fhirRecordDifferenceId);

                return maybeFhirRecordDifference;
            });

        public ValueTask<FhirRecordDifference> ModifyFhirRecordDifferenceAsync(FhirRecordDifference fhirRecordDifference) =>
            TryCatch(async () =>
            {
                fhirRecordDifference = await this.securityAuditBroker.ApplyModifyAuditValuesAsync(fhirRecordDifference);

                await ValidateFhirRecordDifferenceOnModify(fhirRecordDifference);

                FhirRecordDifference maybeFhirRecordDifference =
                    await this.storageBroker.SelectFhirRecordDifferenceByIdAsync(fhirRecordDifference.Id);

                ValidateStorageFhirRecordDifference(maybeFhirRecordDifference, fhirRecordDifference.Id);

                fhirRecordDifference = await this.securityAuditBroker
                    .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(fhirRecordDifference, maybeFhirRecordDifference);

                ValidateAgainstStorageFhirRecordDifferenceOnModify(
                    inputFhirRecordDifference: fhirRecordDifference,
                    storageFhirRecordDifference: maybeFhirRecordDifference);

                return await this.storageBroker.UpdateFhirRecordDifferenceAsync(fhirRecordDifference);
            });

        public ValueTask<FhirRecordDifference> RemoveFhirRecordDifferenceByIdAsync(Guid fhirRecordDifferenceId) =>
            TryCatch(async () =>
            {
                ValidateFhirRecordDifferenceId(fhirRecordDifferenceId);

                FhirRecordDifference maybeFhirRecordDifference = await this.storageBroker
                    .SelectFhirRecordDifferenceByIdAsync(fhirRecordDifferenceId);

                ValidateStorageFhirRecordDifference(maybeFhirRecordDifference, fhirRecordDifferenceId);

                return await this.storageBroker.DeleteFhirRecordDifferenceAsync(maybeFhirRecordDifference);
            });
    }
}