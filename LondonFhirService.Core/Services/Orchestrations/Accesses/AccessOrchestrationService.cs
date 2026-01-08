// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Hashing;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Services.Foundations.Consumers;
using LondonFhirService.Core.Services.Foundations.PdsDatas;

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationService : IAccessOrchestrationService
    {
        private readonly IConsumerService consumerService;
        private readonly IConsumerAccessService consumerAccessService;
        private readonly IPdsDataService pdsDataService;
        private readonly IAuditBroker auditBroker;
        private readonly ISecurityBroker securityBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly IHashBroker hashBroker;
        private readonly AccessConfigurations accessConfigurations;

        public AccessOrchestrationService(
            IConsumerService consumerService,
            IConsumerAccessService consumerAccessService,
            IPdsDataService pdsDataService,
            IAuditBroker auditBroker,
            ISecurityBroker securityBroker,
            IDateTimeBroker dateTimeBroker,
            IIdentifierBroker identifierBroker,
            ILoggingBroker loggingBroker,
            IHashBroker hashBroker,
            AccessConfigurations accessConfigurations)
        {
            this.consumerService = consumerService;
            this.consumerAccessService = consumerAccessService;
            this.pdsDataService = pdsDataService;
            this.auditBroker = auditBroker;
            this.securityBroker = securityBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.identifierBroker = identifierBroker;
            this.loggingBroker = loggingBroker;
            this.hashBroker = hashBroker;
            this.accessConfigurations = accessConfigurations;
        }

        public ValueTask ValidateAccess(string nhsNumber, Guid correlationId) =>
            TryCatch(async () =>
            {
                ValidateNhsNumber(nhsNumber, correlationId);
                User currentUser = await securityBroker.GetCurrentUserAsync();
                string currentUserId = currentUser.UserId;
                IQueryable<Consumer> consumers = await consumerService.RetrieveAllConsumersAsync();
                Consumer matchingConsumer = consumers.FirstOrDefault(consumer => consumer.UserId == currentUserId);

                if (matchingConsumer is null)
                {
                    throw new UnauthorizedAccessOrchestrationException("Current consumer is not a valid consumer.");
                }

                DateTimeOffset now = await dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                bool isActive =
                    matchingConsumer.ActiveFrom <= now
                        && (!matchingConsumer.ActiveTo.HasValue || matchingConsumer.ActiveTo >= now);

                if (!isActive)
                {
                    await this.auditBroker.LogInformationAsync(
                            auditType: "Access",
                            title: "Access Forbidden",

                            message:
                                $"Access was forbidden as consumer with id {matchingConsumer.Id} " +
                                $"is not active / does not have valid access window " +
                                $"(ActiveFrom: {matchingConsumer.ActiveFrom}, ActiveTo: {matchingConsumer.ActiveTo})",

                            fileName: null,
                            correlationId: correlationId.ToString());

                    throw new ForbiddenAccessOrchestrationException(
                        "Current consumer is not active or does not have a valid access window.");
                }

                List<string> consumerActiveOrgs =
                    await consumerAccessService.RetrieveAllActiveOrganisationsUserHasAccessToAsync(matchingConsumer.Id);

                string patientIdentifier = nhsNumber;

                if (accessConfigurations.UseHashedNhsNumber == true)
                {
                    patientIdentifier =
                        await hashBroker.GenerateSha256HashAsync(nhsNumber, accessConfigurations.HashPepper);
                }

                bool organisationsHaveAccessToPatient = await pdsDataService.OrganisationsHaveAccessToThisPatient(
                    nhsNumber: patientIdentifier,
                    organisationCodes: consumerActiveOrgs);

                if (!organisationsHaveAccessToPatient)
                {
                    await this.auditBroker.LogInformationAsync(
                            auditType: "Access",
                            title: "Access Forbidden",
                            message:
                                $"Access was denied as none of the organisations the consumer with id " +
                                $"{matchingConsumer.Id} has access to are permitted to access patient with " +
                                $"NHS number {nhsNumber}.",
                            fileName: null,
                            correlationId: correlationId.ToString());

                    throw new ForbiddenAccessOrchestrationException(
                        "None of the organisations the consumer has access to are permitted to access this patient.");
                }
            });
    }
}
