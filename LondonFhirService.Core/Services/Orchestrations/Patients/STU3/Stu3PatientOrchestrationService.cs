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
    internal partial class Stu3PatientOrchestrationService : IStu3PatientOrchestrationService
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
            Guid? parentId = null,
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

            // The orchestration layer end to end - the figure the "Orchestration Service Request
            // Completed in Nms" audit line used to carry. AccessCheck and ProviderRequests hang
            // off this span rather than off the request root, so subtracting them from it gives
            // orchestration overhead.
            Guid orchestrationSpanId = await this.identifierBroker.GetIdentifierAsync();

            DateTimeOffset orchestrationStarted =
                await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            // Recorded on both exits, so the child spans written underneath it are not left
            // pointing at a parent row that never got inserted when the request fails.
            async ValueTask RecordOrchestrationSpanAsync(MetricStatus status, string errorCode)
            {
                stopwatch.Stop();

                await this.auditAndMetricBroker.LogMetricAsync(new Metric
                {
                    Id = orchestrationSpanId,
                    ParentId = parentId,
                    CorrelationId = correlationId,
                    Method = auditType,
                    Type = MetricType.Orchestration,
                    Name = "Orchestration service request",
                    Started = orchestrationStarted,
                    Completed = orchestrationStarted.AddMilliseconds(stopwatch.Elapsed.TotalMilliseconds),
                    DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                    Status = status,
                    ErrorCode = errorCode
                });
            }

            try
            {
                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Orchestration Service Request Submitted",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());

                await CheckAccessPermissionsAsync(
                    nhsNumber, correlationId, orchestrationSpanId, cancellationToken);

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Retrieve active providers and execute request",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());

                // ProviderRequests wraps discovery and the fan out together, so it is the single
                // figure to set against AccessCheck. Discovery is measured again inside it, as a
                // child, because a slow provider table is a different problem from slow
                // providers.
                Guid providerRequestsSpanId = await this.identifierBroker.GetIdentifierAsync();

                DateTimeOffset providerRequestsStarted =
                    await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                var providerRequestsStopwatch = Stopwatch.StartNew();

                // Recorded on both exits, so the discovery and fan out spans written underneath
                // it are not left pointing at a parent row that never got inserted when a
                // provider call fails.
                async ValueTask RecordSpanAsync(MetricStatus status, string errorCode)
                {
                    providerRequestsStopwatch.Stop();

                    await this.auditAndMetricBroker.LogMetricAsync(new Metric
                    {
                        Id = providerRequestsSpanId,
                        ParentId = orchestrationSpanId,
                        CorrelationId = correlationId,
                        Method = auditType,
                        Type = MetricType.ProviderRequests,
                        Name = "Provider requests",
                        Started = providerRequestsStarted,

                        Completed = providerRequestsStarted
                            .AddMilliseconds(providerRequestsStopwatch.Elapsed.TotalMilliseconds),

                        DurationMs = providerRequestsStopwatch.Elapsed.TotalMilliseconds,
                        Status = status,
                        ErrorCode = errorCode
                    });
                }

                Provider primaryProvider;
                List<Provider> activeProviders;
                List<(string Provider, string Json)> bundles;

                try
                {
                    Guid discoverySpanId = await this.identifierBroker.GetIdentifierAsync();
                    DateTimeOffset discoveryStarted = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
                    var discoveryStopwatch = Stopwatch.StartNew();

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

                    bundles = await this.patientService.GetStructuredRecordSerialisedAsync(
                        activeProviders,
                        correlationId,
                        nhsNumber,
                        dateOfBirth,
                        demographicsOnly,
                        includeInactivePatients,
                        parentId: providerRequestsSpanId,
                        cancellationToken);

                    await RecordSpanAsync(MetricStatus.Succeeded, errorCode: null);
                }
                catch (Exception exception)
                {
                    await RecordSpanAsync(MetricStatus.Failed, exception.GetType().Name);

                    throw;
                }

                stopwatch.Stop();
                long elapsedTime = stopwatch.ElapsedMilliseconds;

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Orchestration Service Request Completed in {elapsedTime}ms",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());

                // Last statement in the try on purpose - see the coordination service's request
                // span for why.
                await RecordOrchestrationSpanAsync(MetricStatus.Succeeded, errorCode: null);

                return new StructuredRecordsResponse
                {
                    PrimaryProvider = primaryProvider,
                    Bundles = bundles
                };
            }
            catch (Exception exception)
            {
                await RecordOrchestrationSpanAsync(MetricStatus.Failed, exception.GetType().Name);

                throw;
            }
        });

        public ValueTask ValidateAccess(
            string nhsNumber,
            Guid correlationId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
            await CheckAccessPermissionsAsync(
                nhsNumber,
                correlationId,
                parentId: null,
                cancellationToken));

        /// <summary>
        /// The access decision itself. Kept separate from the public ValidateAccess so
        /// GetStructuredRecordSerialisedAsync can await it inside its own TryCatch — calling the
        /// public method would localise the same exception twice.
        /// </summary>
        private async ValueTask CheckAccessPermissionsAsync(
            string nhsNumber,
            Guid correlationId,
            Guid? parentId = null,
            CancellationToken cancellationToken = default)
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

                // Recorded on every exit including the failing one. The access dependency being
                // down is precisely the case this span exists to diagnose, and without the catch
                // below a report sliced on Type=AccessCheck would show no rows at all rather than
                // a failure rate.
                async ValueTask RecordAccessCheckSpanAsync(
                    MetricStatus status,
                    string errorCode,
                    string consumer,
                    string description)
                {
                    stopwatch.Stop();

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
                        Status = status,
                        ErrorCode = errorCode,
                        Consumer = consumer,
                        Description = description
                    });
                }

                try
                {
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
                        title: "Check Access Permissions",
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

                        await RecordAccessCheckSpanAsync(
                            MetricStatus.Failed,
                            errorCode: "AccessForbidden",
                            consumer: currentUser.UserId,
                            description: "Access denied.");

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

                    await RecordAccessCheckSpanAsync(
                        MetricStatus.Succeeded,
                        errorCode: null,
                        consumer: currentUser.UserId,

                        description:
                            $"Allowed via {consumerAccess.AllowedViaOrganisations.Count} organisation(s).");
                }
                catch (ForbiddenPatientOrchestrationException)
                {
                    // Already recorded as Failed/AccessForbidden by the branch that threw it.
                    throw;
                }
                catch (Exception exception)
                {
                    await RecordAccessCheckSpanAsync(
                        MetricStatus.Failed,
                        errorCode: exception.GetType().Name,
                        consumer: null,
                        description: "Access check did not complete.");

                    throw;
                }
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
            // Materialised by the broker rather than enumerated here. The service returns a
            // deferred queryable, so a synchronous ToList would run the round trip on this thread
            // and park a thread-pool worker inside reader I/O for its duration.
            List<Provider> allProviders =
                await this.providerService.RetrieveAllProvidersAsListAsync();

            List<Provider> orderedProviders = allProviders
                .Where(provider => provider.FhirVersion == "STU3")
                .OrderByDescending(provider => provider.IsPrimary)
                .ToList();

            // Via the broker, like every other clock read in this class. Read inline, the provider
            // activation window - the rule deciding which providers a live request fans out to -
            // could not be controlled from the injected clock, so no test could pin it.
            DateTimeOffset now = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

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
