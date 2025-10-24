// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.OdsDatas;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessService : IConsumerAccessService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public ConsumerAccessService(
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

        public ValueTask<ConsumerAccess> AddConsumerAccessAsync(ConsumerAccess consumerAccess) =>
        TryCatch(async () =>
        {
            ConsumerAccess consumerAccessWithAddAuditApplied =
                await this.securityAuditBroker.ApplyAddAuditValuesAsync(consumerAccess);

            await ValidateConsumerAccessOnAddAsync(consumerAccessWithAddAuditApplied);

            return await this.storageBroker.InsertConsumerAccessAsync(consumerAccessWithAddAuditApplied);
        });

        public ValueTask<IQueryable<ConsumerAccess>> RetrieveAllConsumerAccessesAsync() =>
        TryCatch(this.storageBroker.SelectAllConsumerAccessesAsync);

        public ValueTask<ConsumerAccess> RetrieveConsumerAccessByIdAsync(Guid consumerAccessId) =>
        TryCatch(async () =>
        {
            ValidateConsumerAccessOnRetrieveById(consumerAccessId);

            var maybeConsumerAccess = await this.storageBroker
                .SelectConsumerAccessByIdAsync(consumerAccessId);

            ValidateStorageConsumerAccess(maybeConsumerAccess, consumerAccessId);

            return maybeConsumerAccess;
        });

        public ValueTask<ConsumerAccess> ModifyConsumerAccessAsync(ConsumerAccess consumerAccess) =>
        TryCatch(async () =>
        {
            ConsumerAccess consumerAccessWithModifyAuditApplied =
                await this.securityAuditBroker.ApplyModifyAuditValuesAsync(consumerAccess);

            await ValidateConsumerAccessOnModifyAsync(consumerAccessWithModifyAuditApplied);

            var maybeConsumerAccess = await this.storageBroker
                .SelectConsumerAccessByIdAsync(consumerAccessWithModifyAuditApplied.Id);

            ValidateStorageConsumerAccess(maybeConsumerAccess, consumerAccessWithModifyAuditApplied.Id);

            await ValidateAgainstStorageConsumerAccessOnModifyAsync(
                consumerAccessWithModifyAuditApplied, maybeConsumerAccess);

            return await this.storageBroker.UpdateConsumerAccessAsync(consumerAccessWithModifyAuditApplied);
        });

        public ValueTask<ConsumerAccess> RemoveConsumerAccessByIdAsync(Guid consumerAccessId) =>
        TryCatch(async () =>
        {
            ValidateConsumerAccessOnRemoveById(consumerAccessId);

            var maybeConsumerAccess = await this.storageBroker
                .SelectConsumerAccessByIdAsync(consumerAccessId);

            ValidateStorageConsumerAccess(maybeConsumerAccess, consumerAccessId);

            ConsumerAccess consumerAccessWithModifyAuditApplied =
                await this.securityAuditBroker.ApplyRemoveAuditValuesAsync(maybeConsumerAccess);

            var updatedConsumerAccess = await this.storageBroker
                .UpdateConsumerAccessAsync(consumerAccessWithModifyAuditApplied);

            await ValidateAgainstStorageConsumerAccessOnDeleteAsync(updatedConsumerAccess, consumerAccessWithModifyAuditApplied);

            return await this.storageBroker.DeleteConsumerAccessAsync(updatedConsumerAccess);
        });

        public ValueTask<List<string>> RetrieveAllActiveOrganisationsUserHasAccessToAsync(Guid consumerId) =>
        TryCatch(async () =>
        {
            ValidateOnRetrieveAllOrganisationUserHasAccessTo(consumerId);
            List<string> organisations = new List<string>();
            var consumerAccessQuery = await this.storageBroker.SelectAllConsumerAccessesAsync();
            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            List<string> userOrganisations = consumerAccessQuery
                .Where(consumerAccess => consumerAccess.ConsumerId == consumerId)
                    .Select(consumerAccess => consumerAccess.OrgCode).Distinct().ToList();

            foreach (var userOrganisation in userOrganisations)
            {
                IQueryable<OdsData> odsParentRecord =
                    await this.storageBroker.SelectAllOdsDatasAsync();

                OdsData parentRecord = odsParentRecord
                    .FirstOrDefault(ods => ods.OrganisationCode == userOrganisation);

                if (parentRecord != null)
                {
                    organisations.Add(parentRecord.OrganisationCode);

                    IQueryable<OdsData> odsDataQuery =
                        await this.storageBroker.SelectAllOdsDatasAsync();

                    odsDataQuery = odsDataQuery
                        .Where(ods => ods.OdsHierarchy.IsDescendantOf(parentRecord.OdsHierarchy)
                            && (ods.RelationshipWithParentStartDate == null
                                || ods.RelationshipWithParentStartDate <= currentDateTime)
                            && (ods.RelationshipWithParentEndDate == null ||
                                ods.RelationshipWithParentEndDate > currentDateTime));

                    List<string> descendants = odsDataQuery.ToList()
                        .Select(odsData => odsData.OrganisationCode).ToList();

                    organisations.AddRange(descendants);
                }
            }

            return organisations.Distinct().ToList();
        });
    }
}
