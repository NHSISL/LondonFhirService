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
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Metrics;
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
        private readonly IAuditAndMetricBroker auditAndMetricBroker;
        private readonly ISecurityBroker securityBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly AccessConfigurations accessConfigurations;

        public Stu3PatientOrchestrationService(
            IProviderService providerService,
            IStu3PatientService patientService,
            IConsumerAccessService consumerAccessService,
            IAuditAndMetricBroker auditAndMetricBroker,
            ISecurityBroker securityBroker,
            ILoggingBroker loggingBroker,
            IIdentifierBroker identifierBroker,
            IDateTimeBroker dateTimeBroker,
            AccessConfigurations accessConfigurations)
        {
            this.providerService = providerService;
            this.patientService = patientService;
            this.consumerAccessService = consumerAccessService;
            this.auditAndMetricBroker = auditAndMetricBroker;
            this.securityBroker = securityBroker;
            this.loggingBroker = loggingBroker;
            this.identifierBroker = identifierBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.accessConfigurations = accessConfigurations;
        }

        public ValueTask<StructuredRecordsResponse> GetStructuredRecordSerialisedAsync(
            Guid correlationId,
            string nhsNumber,
            string dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default,
            Guid? parentId = null) =>
        TryCatch(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            ValidateArgsOnGetStructuredRecord(nhsNumber, correlationId);
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditAndMetricBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Submitted",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            await CheckAccessPermissionsAsync(nhsNumber, correlationId, cancellationToken, parentId);

            await this.auditAndMetricBroker.LogInformationAsync(
                auditType,
                title: $"Retrieve active providers and execute request",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            // ProviderRequests wraps discovery and the fan out together, so it is the single
            // figure to set against AccessCheck and Consolidation. Discovery is measured again
            // inside it, as a child, because a slow provider table is a different problem from
            // slow providers.
            Guid providerRequestsSpanId = await this.identifierBroker.GetIdentifierAsync();

            DateTimeOffset providerRequestsStarted =
                await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            var providerRequestsStopwatch = Stopwatch.StartNew();
            Guid discoverySpanId = await this.identifierBroker.GetIdentifierAsync();
            DateTimeOffset discoveryStarted = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            var discoveryStopwatch = Stopwatch.StartNew();

            Provider primaryProvider;
            List<Provider> activeProviders;
            (primaryProvider, activeProviders) = await GetProviderInfo();

            discoveryStopwatch.Stop();

            await this.auditAndMetricBroker.LogMetricAsync(new Metric
            {
                Id = discoverySpanId,
                ParentId = providerRequestsSpanId,
                CorrelationId = correlationId,
                Method = auditType,
                Type = MetricType.ProviderDiscovery,
                Name = "Resolve active providers",
                Started = discoveryStarted,
                Completed = discoveryStarted.AddMilliseconds(discoveryStopwatch.Elapsed.TotalMilliseconds),
                DurationMs = discoveryStopwatch.Elapsed.TotalMilliseconds,
                Status = MetricStatus.Succeeded,
                Target = primaryProvider?.FullyQualifiedName,
                Description = $"{activeProviders.Count} active STU3 provider(s) resolved."
            });

            List<(string Provider, string Json)> bundles = await this.patientService.GetStructuredRecordSerialisedAsync(
                activeProviders,
                correlationId,
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken,
                parentId: providerRequestsSpanId);

            providerRequestsStopwatch.Stop();

            await this.auditAndMetricBroker.LogMetricAsync(new Metric
            {
                Id = providerRequestsSpanId,
                ParentId = parentId,
                CorrelationId = correlationId,
                Method = auditType,
                Type = MetricType.ProviderRequests,
                Name = "Provider requests",
                Started = providerRequestsStarted,

                Completed = providerRequestsStarted
                    .AddMilliseconds(providerRequestsStopwatch.Elapsed.TotalMilliseconds),

                DurationMs = providerRequestsStopwatch.Elapsed.TotalMilliseconds,
                Status = MetricStatus.Succeeded,
                Description = $"{bundles.Count} of {activeProviders.Count} provider(s) returned a bundle."
            });

            stopwatch.Stop();
            long elapsedTime = stopwatch.ElapsedMilliseconds;

            await this.auditAndMetricBroker.LogInformationAsync(
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
            CancellationToken cancellationToken = default,
            Guid? parentId = null)
        {
            ValidateArgsOnValidateAccess(nhsNumber, correlationId);
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string message = $"Parameters:  {{ nhsNumber = \"{nhsNumber}\" }}";
            Guid accessCheckSpanId = await this.identifierBroker.GetIdentifierAsync();

            DateTimeOffset accessCheckStarted =
                await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            if (this.accessConfigurations.CheckAccessPermissions)
            {
                var stopwatch = Stopwatch.StartNew();

                await this.auditAndMetricBroker.LogInformationAsync(
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

                await this.auditAndMetricBroker.LogInformationAsync(
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

                    // Awaited, unlike the operational tracing around it. An access decision is
                    // the compliance record of who read a patient's record; it must not be lost
                    // to a process restart, and a failure to write it must surface.
                    await this.auditAndMetricBroker.RecordAuditAsync(
                        auditType: "Access",
                        title: "Access Forbidden",

                        message:
                            $"Access was denied as consumer with id {currentUser.UserId} is not permitted " +
                            $"to access patient with NHS number {nhsNumber}. Reasons: {reasons}  " +
                            $"CorrelationId: {correlationId.ToString()}, ElapsedTime: {elapsedTime}ms",

                        fileName: null,
                        correlationId: correlationId.ToString());

                    await this.auditAndMetricBroker.LogMetricAsync(new Metric
                    {
                        Id = accessCheckSpanId,
                        ParentId = parentId,
                        CorrelationId = correlationId,
                        Method = auditType,
                        Type = MetricType.AccessCheck,
                        Name = "Check access permissions",
                        Started = accessCheckStarted,
                        Completed = accessCheckStarted.AddMilliseconds(stopwatch.Elapsed.TotalMilliseconds),
                        DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                        Status = MetricStatus.Failed,
                        ErrorCode = "AccessForbidden",
                        Consumer = currentUser.UserId,
                        Description = "Access denied."
                    });

                    throw new ForbiddenPatientOrchestrationException(
                        "Current consumer is not permitted to access this patient.  " +
                        $"CorrelationId: {correlationId.ToString()}");
                }

                // Awaited - see the forbidden branch above.
                await this.auditAndMetricBroker.RecordAuditAsync(
                    auditType: "Access",
                    title: "Access Allowed",

                    message:
                        $"{currentUser.UserId} is allowed to access patient with " +
                        $"NHS number {nhsNumber} via org codes: " +
                        $"{string.Join(", ", consumerAccess.AllowedViaOrganisations)}  " +
                        $"CorrelationId: {correlationId.ToString()}, ElapsedTime: {elapsedTime}ms",

                    fileName: null,
                    correlationId: correlationId.ToString());

                await this.auditAndMetricBroker.LogMetricAsync(new Metric
                {
                    Id = accessCheckSpanId,
                    ParentId = parentId,
                    CorrelationId = correlationId,
                    Method = auditType,
                    Type = MetricType.AccessCheck,
                    Name = "Check access permissions",
                    Started = accessCheckStarted,
                    Completed = accessCheckStarted.AddMilliseconds(stopwatch.Elapsed.TotalMilliseconds),
                    DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                    Status = MetricStatus.Succeeded,
                    Consumer = currentUser.UserId,

                    Description =
                        $"Allowed via {consumerAccess.AllowedViaOrganisations.Count} organisation(s)."
                });
            }
            else
            {
                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Access permission check skipped due to configuration (CheckAccessPermissions = false)",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());

                // Recorded rather than omitted, so a request with no access check is visibly a
                // configuration choice rather than a gap in the trace.
                await this.auditAndMetricBroker.LogMetricAsync(new Metric
                {
                    Id = accessCheckSpanId,
                    ParentId = parentId,
                    CorrelationId = correlationId,
                    Method = auditType,
                    Type = MetricType.AccessCheck,
                    Name = "Check access permissions",
                    Started = accessCheckStarted,
                    Completed = accessCheckStarted,
                    DurationMs = 0,
                    Status = MetricStatus.Skipped,
                    Description = "Skipped: CheckAccessPermissions is false."
                });
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
