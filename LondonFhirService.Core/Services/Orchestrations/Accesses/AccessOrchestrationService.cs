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
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Services.Foundations.Consumers;
using LondonFhirService.Core.Services.Foundations.PdsDatas;

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public class AccessOrchestrationService : IAccessOrchestrationService
    {
        private readonly IConsumerService consumerService;
        private readonly IConsumerAccessService consumerAccessService;
        private readonly IPdsDataService pdsDataService;
        private readonly IAuditBroker auditBroker;
        private readonly ISecurityBroker securityBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly IIdentifierBroker identifierBroker;

        public AccessOrchestrationService(
            IConsumerService consumerService,
            IConsumerAccessService consumerAccessService,
            IPdsDataService pdsDataService,
            IAuditBroker auditBroker,
            ISecurityBroker securityBroker,
            IDateTimeBroker dateTimeBroker,
            IIdentifierBroker identifierBroker)
        {
            this.consumerService = consumerService;
            this.consumerAccessService = consumerAccessService;
            this.pdsDataService = pdsDataService;
            this.auditBroker = auditBroker;
            this.securityBroker = securityBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.identifierBroker = identifierBroker;
        }

        public async ValueTask ValidateAccess(string nhsNumber)
        {
            User currentUser = await securityBroker.GetCurrentUserAsync();
            string currentUserId = currentUser.UserId;
            IQueryable<Consumer> consumers = await consumerService.RetrieveAllConsumersAsync();
            Consumer matchingConsumer = consumers.FirstOrDefault(consumer => consumer.UserId == currentUserId);

            if (matchingConsumer is null)
            {
                throw new UnauthorizedAccessOrchestrationException("Current consumer is not a valid consumer.");
            }

            Guid correlationId = await this.identifierBroker.GetIdentifierAsync();
            DateTimeOffset now = await dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            bool isFhirServiceApiConsumer = await securityBroker.IsInRoleAsync("LondonFhirServiceApiConsumer");

            if (!isFhirServiceApiConsumer || matchingConsumer.ActiveFrom > now || matchingConsumer.ActiveTo < now)
            {
                await this.auditBroker.LogInformationAsync(
                        auditType: "Access",
                        title: "Access Forbidden",
                        message:
                            $"Access was forbidden as consumer with id {matchingConsumer.Id} " +
                            $"is inactive or does not have the required role.",
                        fileName: null,
                        correlationId: correlationId.ToString());

                throw new ForbiddenAccessOrchestrationException(
                    "Current consumer is not active or does not have the required role.");
            }

            List<string> consumerActiveOrgs =
                await consumerAccessService.RetrieveActiveOrganisationsConsumerHasAccessToAsync(matchingConsumer.Id);

            bool organisationsHaveAccessToPatient = await pdsDataService.OrganisationsHaveAccessToThisPatient(
                nhsNumber: nhsNumber,
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
        }
    }
}
