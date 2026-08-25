// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;

namespace LondonFhirService.Core.Services.Foundations.FhirRecordDifferences
{
    internal partial class FhirRecordDifferenceService : IFhirRecordDifferenceService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public FhirRecordDifferenceService(
            IStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<FhirRecordDifference> AddFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                fhirRecordDifference = await this.securityAuditBroker.ApplyAddAuditValuesAsync(fhirRecordDifference);
                await ValidateFhirRecordDifferenceOnAdd(fhirRecordDifference);

                return await this.storageBroker.InsertFhirRecordDifferenceAsync(fhirRecordDifference, cancellationToken);
            });

        public ValueTask<IQueryable<FhirRecordDifference>> RetrieveAllFhirRecordDifferencesAsync(
            CancellationToken cancellationToken = default) =>
            TryCatch(async () => await this.storageBroker.SelectAllFhirRecordDifferencesAsync(cancellationToken));

        public ValueTask<FhirRecordDifference> RetrieveFhirRecordDifferenceByIdAsync(
            Guid fhirRecordDifferenceId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateFhirRecordDifferenceId(fhirRecordDifferenceId);

                FhirRecordDifference maybeFhirRecordDifference = await this.storageBroker
                    .SelectFhirRecordDifferenceByIdAsync(fhirRecordDifferenceId, cancellationToken);

                ValidateStorageFhirRecordDifference(maybeFhirRecordDifference, fhirRecordDifferenceId);

                return maybeFhirRecordDifference;
            });

        public ValueTask<FhirRecordDifference> ModifyFhirRecordDifferenceAsync(
            FhirRecordDifference fhirRecordDifference,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                fhirRecordDifference = await this.securityAuditBroker.ApplyModifyAuditValuesAsync(fhirRecordDifference);

                await ValidateFhirRecordDifferenceOnModify(fhirRecordDifference);

                FhirRecordDifference maybeFhirRecordDifference =
                    await this.storageBroker.SelectFhirRecordDifferenceByIdAsync(fhirRecordDifference.Id, cancellationToken);

                ValidateStorageFhirRecordDifference(maybeFhirRecordDifference, fhirRecordDifference.Id);

                fhirRecordDifference = await this.securityAuditBroker
                    .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(fhirRecordDifference, maybeFhirRecordDifference);

                ValidateAgainstStorageFhirRecordDifferenceOnModify(
                    inputFhirRecordDifference: fhirRecordDifference,
                    storageFhirRecordDifference: maybeFhirRecordDifference);

                return await this.storageBroker.UpdateFhirRecordDifferenceAsync(fhirRecordDifference, cancellationToken);
            });

        public ValueTask<FhirRecordDifference> RemoveFhirRecordDifferenceByIdAsync(
            Guid fhirRecordDifferenceId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateFhirRecordDifferenceId(fhirRecordDifferenceId);

                FhirRecordDifference maybeFhirRecordDifference = await this.storageBroker
                    .SelectFhirRecordDifferenceByIdAsync(fhirRecordDifferenceId, cancellationToken);

                ValidateStorageFhirRecordDifference(maybeFhirRecordDifference, fhirRecordDifferenceId);

                return await this.storageBroker.DeleteFhirRecordDifferenceAsync(maybeFhirRecordDifference, cancellationToken);
            });
    }
}