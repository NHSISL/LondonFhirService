// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Hashing;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationService : IAccessOrchestrationService
    {
        private readonly IConsumerAccessService consumerAccessService;
        private readonly IAuditBroker auditBroker;
        private readonly ISecurityBroker securityBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly IHashBroker hashBroker;
        private readonly AccessConfigurations accessConfigurations;

        public AccessOrchestrationService(
            IConsumerAccessService consumerAccessService,
            IAuditBroker auditBroker,
            ISecurityBroker securityBroker,
            IDateTimeBroker dateTimeBroker,
            IIdentifierBroker identifierBroker,
            ILoggingBroker loggingBroker,
            IHashBroker hashBroker,
            AccessConfigurations accessConfigurations)
        {
            this.consumerAccessService = consumerAccessService;
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
                var stopwatch = Stopwatch.StartNew();
                ValidateNhsNumber(nhsNumber, correlationId);
                User currentUser = await securityBroker.GetCurrentUserAsync();

                if (currentUser is null)
                {
                    throw new UnauthorizedAccessOrchestrationException($"Current consumer is not a valid consumer.");
                }

                string currentUserId = currentUser.UserId;

                JsonSerializerOptions options = new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };

                string currentUserJson = JsonSerializer.Serialize(currentUser, options);

                await this.auditBroker.LogInformationAsync(
                    auditType: "Access",
                    title: "Check Access Permissons",
                    message: currentUserJson,
                    fileName: null,
                    correlationId: correlationId.ToString());

                ConsumerAccess consumerAccess = await consumerAccessService.CheckConsumerAccessAsync(
                    nhsNumber,
                    correlationId,
                    cancellationToken: default);

                stopwatch.Stop();
                long elapsedTime = stopwatch.ElapsedMilliseconds;

                if (consumerAccess is null)
                {
                    throw new UnauthorizedAccessOrchestrationException(
                        $"Current consumer with id `{currentUserId}` is not a valid consumer.");
                }

                if (consumerAccess.IsAccessAllowed == false)
                {
                    UnauthorizedAccessOrchestrationException unauthorizedAccessOrchestrationException =
                        new UnauthorizedAccessOrchestrationException(
                            $"Current consumer with id `{currentUserId}` is not allowed access.");

                    foreach (AccessReason reason in consumerAccess.Reasons)
                    {
                        unauthorizedAccessOrchestrationException.Data.Add(
                            key: reason.Code,
                            value: reason.Message);
                    }

                    throw unauthorizedAccessOrchestrationException;
                }

                string consumerAccessJson = JsonSerializer.Serialize(consumerAccess, options);

                await this.auditBroker.LogInformationAsync(
                    auditType: "Access",
                    title: "Access Allowed",

                    message:
                        $"{currentUserId} is allowed to access patient with " +
                        $"NHS number {nhsNumber} via " + Environment.NewLine +

                        $"Subscriber Aggreement(s): " +
                            $"{string.Join(", ", consumerAccess.AllowedViaInformationSharingAgreements)}, " +
                                Environment.NewLine +

                        $"Org Codes: {string.Join(", ", consumerAccess.AllowedViaOrganisations)},  " +
                            Environment.NewLine +

                        $"Consumer Access: " + Environment.NewLine + $"{consumerAccessJson}, " + Environment.NewLine +
                        $"CorrelationId: {correlationId.ToString()}, ElapsedTime: {elapsedTime}ms",

                    fileName: null,
                    correlationId: correlationId.ToString());
            });
    }
}
