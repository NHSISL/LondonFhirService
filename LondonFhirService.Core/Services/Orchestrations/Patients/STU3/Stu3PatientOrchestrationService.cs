// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Models.Orchestrations.Patients;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using LondonFhirService.Core.Services.Foundations.Providers;

namespace LondonFhirService.Core.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationService : IStu3PatientOrchestrationService
    {
        private readonly IProviderService providerService;
        private readonly IStu3PatientService patientService;
        private readonly IConsumerAccessService consumerAccessService;
        private readonly IAuditBroker auditBroker;
        private readonly ISecurityBroker securityBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly AccessConfigurations accessConfigurations;

        public Stu3PatientOrchestrationService(
            IProviderService providerService,
            IStu3PatientService patientService,
            IConsumerAccessService consumerAccessService,
            IAuditBroker auditBroker,
            ISecurityBroker securityBroker,
            ILoggingBroker loggingBroker,
            AccessConfigurations accessConfigurations)
        {
            this.providerService = providerService;
            this.patientService = patientService;
            this.consumerAccessService = consumerAccessService;
            this.auditBroker = auditBroker;
            this.securityBroker = securityBroker;
            this.loggingBroker = loggingBroker;
            this.accessConfigurations = accessConfigurations;
        }

        public ValueTask<StructuredRecordsResponse> GetStructuredRecordSerialisedAsync(
            Guid correlationId,
            string nhsNumber,
            string dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            ValidateArgsOnGetStructuredRecord(nhsNumber, correlationId);
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Submitted",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            await CheckAccessPermissionsAsync(nhsNumber, correlationId, cancellationToken);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Retrieve active providers and execute request",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            Provider primaryProvider;
            List<Provider> activeProviders;
            (primaryProvider, activeProviders) = await GetProviderInfo();

            List<(string Provider, string Json)> bundles = await this.patientService.GetStructuredRecordSerialisedAsync(
                activeProviders,
                correlationId,
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken);

            stopwatch.Stop();
            long elapsedTime = stopwatch.ElapsedMilliseconds;

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Completed in {elapsedTime}ms",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            return new StructuredRecordsResponse
            {
                PrimaryProvider = primaryProvider,
                Bundles = bundles
            };
        });

        public ValueTask ValidateAccess(
            string nhsNumber,
            Guid correlationId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
            await CheckAccessPermissionsAsync(nhsNumber, correlationId, cancellationToken));

        /// <summary>
        /// The access decision itself. Kept separate from the public ValidateAccess so
        /// GetStructuredRecordSerialisedAsync can await it inside its own TryCatch — calling the
        /// public method would localise the same exception twice.
        /// </summary>
        private async ValueTask CheckAccessPermissionsAsync(
            string nhsNumber,
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            ValidateArgsOnValidateAccess(nhsNumber, correlationId);
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string message = $"Parameters:  {{ nhsNumber = \"{nhsNumber}\" }}";

            if (this.accessConfigurations.CheckAccessPermissions)
            {
                var stopwatch = Stopwatch.StartNew();

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Check Access Permissions",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());

                User currentUser = await this.securityBroker.GetCurrentUserAsync();

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

                if (currentUser is null)
                {
                    throw new UnauthorizedPatientOrchestrationException(
                        $"Current consumer is not a valid consumer.");
                }

                ValidateAccessRequest validateAccessRequest = new ValidateAccessRequest
                {
                    ConsumerUserId = currentUser.UserId,
                    NhsNumber = nhsNumber,
                    CorrelationId = correlationId
                };

                ConsumerAccess consumerAccess =
                    await this.consumerAccessService.CheckConsumerAccessAsync(
                        validateAccessRequest, cancellationToken);

                stopwatch.Stop();
                long elapsedTime = stopwatch.ElapsedMilliseconds;

                if (consumerAccess.IsAccessAllowed is false)
                {
                    string reasons = string.Join(", ", consumerAccess.Reasons
                        .Select(reason => $"{reason.Code}: {reason.Message}"));

                    await this.auditBroker.LogInformationAsync(
                        auditType: "Access",
                        title: "Access Forbidden",

                        message:
                            $"Access was denied as consumer with id {currentUser.UserId} is not permitted " +
                            $"to access patient with NHS number {nhsNumber}. Reasons: {reasons}  " +
                            $"CorrelationId: {correlationId.ToString()}, ElapsedTime: {elapsedTime}ms",

                        fileName: null,
                        correlationId: correlationId.ToString());

                    throw new ForbiddenPatientOrchestrationException(
                        "Current consumer is not permitted to access this patient.  " +
                        $"CorrelationId: {correlationId.ToString()}");
                }

                await this.auditBroker.LogInformationAsync(
                    auditType: "Access",
                    title: "Access Allowed",

                    message:
                        $"{currentUser.UserId} is allowed to access patient with " +
                        $"NHS number {nhsNumber} via org codes: " +
                        $"{string.Join(", ", consumerAccess.AllowedViaOrganisations)}  " +
                        $"CorrelationId: {correlationId.ToString()}, ElapsedTime: {elapsedTime}ms",

                    fileName: null,
                    correlationId: correlationId.ToString());
            }
            else
            {
                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Access permission check skipped due to configuration (CheckAccessPermissions = false)",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());
            }
        }

        private async ValueTask<(Provider primaryProvider, List<Provider> activeProvider)> GetProviderInfo()
        {
            IQueryable<Provider> allProviders =
                await this.providerService.RetrieveAllProvidersAsync();

            List<Provider> orderedProviders = allProviders
                .Where(provider => provider.FhirVersion == "STU3")
                .OrderByDescending(provider => provider.IsPrimary)
                .ToList();

            DateTimeOffset now = DateTimeOffset.UtcNow;

            List<Provider> primaryProviders = orderedProviders
                .Where(provider =>
                    provider.IsPrimary &&
                    provider.IsActive &&
                    (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                    (provider.ActiveTo == null || provider.ActiveTo >= now))
                .ToList();

            ValidatePrimaryProviders(primaryProviders);
            Provider primaryProvider = primaryProviders.First();

            List<Provider> activeProviders = orderedProviders
                .Where(provider =>
                    provider.IsActive &&
                    (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                    (provider.ActiveTo == null || provider.ActiveTo >= now))
                .ToList();

            return (primaryProvider, activeProviders);
        }
    }
}
