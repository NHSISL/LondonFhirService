// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Fhirs.STU3;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Extensions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Services.Foundations.Patients.STU3
{
    internal partial class Stu3PatientService : IStu3PatientService
    {
        private readonly IStu3FhirBroker fhirBroker;
        private readonly IAuditAndMetricBroker auditAndMetricBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly IStorageBrokerFactory storageBrokerFactory;
        private readonly IAuditAndMetricsDispatcher dispatcher;
        private readonly PatientServiceConfig patientServiceConfig;

        public Stu3PatientService(
            IStu3FhirBroker fhirBroker,
            IAuditAndMetricBroker auditAndMetricBroker,
            IIdentifierBroker identifierBroker,
            IDateTimeBroker dateTimeBroker,
            ISecurityAuditBroker securityAuditBroker,
            IStorageBrokerFactory storageBrokerFactory,
            IAuditAndMetricsDispatcher dispatcher,
            ILoggingBroker loggingBroker,
            PatientServiceConfig patientServiceConfig)
        {
            this.fhirBroker = fhirBroker;
            this.auditAndMetricBroker = auditAndMetricBroker;
            this.identifierBroker = identifierBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.storageBrokerFactory = storageBrokerFactory;
            this.dispatcher = dispatcher;
            this.loggingBroker = loggingBroker;
            this.patientServiceConfig = patientServiceConfig;
        }

        public ValueTask<List<(string Provider, string Json)>> GetStructuredRecordSerialisedAsync(
            List<Provider> activeProviders,
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
                dateOfBirth = string.IsNullOrWhiteSpace(dateOfBirth) ? null : dateOfBirth.Trim();
                ValidateOnGetStructuredRecord(activeProviders, nhsNumber, dateOfBirth, correlationId);
                string auditType = "STU3-Patient-GetStructuredRecordSerialised";

                string message =
                    $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                    $"demographicsOnly = \"{demographicsOnly}\", " +
                    $"includeInactivePatients = \"{includeInactivePatients}\" }}";

                // No Foundation span is recorded. It measured this layer end to end, which sat
                // between ProviderRequests and ProviderFanOut and differed from its parent only
                // by the cost of resolving providers and assembling the outcomes. The fan out now
                // hangs directly off ProviderRequests.

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Foundation Service Request Submitted",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());

                List<(string providerFriendlyName, bool isPrimaryProvider, IFhirProvider provider)> fhirProviders =
                    await GetFhirProviders(activeProviders);

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Parallel Provider Execution Started",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());

                // The fan out span is the parent of every provider task, so subtracting a
                // Provider span from it gives the time that provider's result sat idle
                // waiting for the slowest sibling.
                Guid fanOutSpanId = await this.identifierBroker.GetIdentifierAsync();

                // Both origins are taken here, before ToArray materialises the tasks. The
                // wall clock and the stopwatch then measure from the same instant, and no
                // child can start before the parent it hangs from.
                DateTimeOffset fanOutStarted = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
                var stopwatchOutcomes = Stopwatch.StartNew();

                // Recorded on both exits. Individual provider failures already have their own
                // spans; this covers the barrier itself failing, which would orphan them.
                async ValueTask RecordFanOutSpanAsync(MetricStatus status, string errorCode)
                {
                    stopwatchOutcomes.Stop();

                    await this.auditAndMetricBroker.LogMetricAsync(new Metric
                    {
                        Id = fanOutSpanId,
                        ParentId = parentId,
                        CorrelationId = correlationId,
                        Method = auditType,
                        Type = MetricType.ProviderFanOut,
                        Name = "Parallel provider execution",
                        Started = fanOutStarted,
                        Completed = fanOutStarted.AddMilliseconds(stopwatchOutcomes.Elapsed.TotalMilliseconds),
                        DurationMs = stopwatchOutcomes.Elapsed.TotalMilliseconds,
                        Status = status,
                        ErrorCode = errorCode,
                        Description = $"{fhirProviders.Count} provider task(s) awaited."
                    });
                }

                (string Provider, string Json, Exception Exception)[] outcomes;

                try
                {
                    var tasks = fhirProviders.Select(fhirProviders => ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                        fhirProviders.providerFriendlyName,
                        fhirProviders.isPrimaryProvider,
                        fhirProviders.provider,
                        correlationId,
                        nhsNumber,
                        dateOfBirth,
                        demographicsOnly,
                        includeInactivePatients,
                        fanOutSpanId,
                        cancellationToken)).ToArray();

                    outcomes = await Task.WhenAll(tasks).ConfigureAwait(false);
                    await RecordFanOutSpanAsync(MetricStatus.Succeeded, errorCode: null);
                }
                catch (Exception exception)
                {
                    await RecordFanOutSpanAsync(MetricStatus.Failed, exception.GetType().Name);

                    throw;
                }

            await this.auditAndMetricBroker.LogInformationAsync(
                auditType,
                title: $"Parallel Provider Execution Completed in {stopwatchOutcomes.ElapsedMilliseconds}ms",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            var jsonBundles = new List<(string, string)>(outcomes.Length);
            var exceptions = new List<Exception>();

            foreach (var outcome in outcomes)
            {
                if (outcome.Json is not null)
                {
                    jsonBundles.Add((outcome.Provider, outcome.Json));
                }
                else if (outcome.Exception is not null)
                {
                    exceptions.Add(outcome.Exception);
                }
            }

            if (exceptions.Count > 0)
            {
                var aggregate = new AggregateException(
                    "One or more provider calls failed or timed out.",
                    exceptions);

                await loggingBroker.LogErrorAsync(aggregate);
            }

            stopwatch.Stop();
            long elapsedTime = stopwatch.ElapsedMilliseconds;

            await this.auditAndMetricBroker.LogInformationAsync(
                auditType,
                title: $"Foundation Service Request Completed in {elapsedTime}ms",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            return jsonBundles;
            });

        private async ValueTask<List<(string providerFriendlyName, bool isPrimaryProvider, IFhirProvider provider)>>
            GetFhirProviders(List<Provider> activeProviders)
        {
            List<(string providerFriendlyName, bool isPrimaryProvider, IFhirProvider provider)> fhirProviders =
                new List<(string providerFriendlyName, bool isPrimaryProvider, IFhirProvider provider)>();

            // Forward, so the primary-first ordering established by GetProviderInfo survives into
            // the fan out. Nothing selects a provider by position any more, but a list that
            // silently reverses its caller's ordering is a trap worth not leaving lying around.
            for (int i = 0; i < activeProviders.Count; i++)
            {
                var provider = fhirBroker.FhirProviders
                    .Where(provider => provider.ProviderName == activeProviders[i].FullyQualifiedName)
                    .FirstOrDefault();

                try
                {
                    // Checked before the call rather than after. SupportsResource on a null
                    // provider throws, and the generic catch below then logs a
                    // NullReferenceException instead of the configuration problem it actually is
                    // - a provider row naming a provider this host does not have.
                    if (provider is null)
                    {
                        await loggingBroker.LogInformationAsync(
                            $"Removing '{activeProviders[i].FriendlyName}' as " +
                                $"'{activeProviders[i].FullyQualifiedName}' not found.");

                        continue;
                    }

                    if (provider.SupportsResource("Patients", "GetStructuredRecordAsync") == false)
                    {
                        await loggingBroker.LogInformationAsync($"Removing '{provider.ProviderName}': " +
                            "Patients/$GetStructuredRecord not supported.");

                        continue;
                    }

                    fhirProviders.Add((activeProviders[i].FriendlyName, activeProviders[i].IsPrimary, provider));
                }
                catch (Exception exception)
                {
                    await loggingBroker.LogErrorAsync(exception);
                }
            }

            return fhirProviders;
        }

        virtual internal async Task<(string Provider, string Json, Exception Exception)>
            ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                string providerFriendlyName,
                bool isPrimaryProvider,
                IFhirProvider provider,
                Guid correlationId,
                string nhsNumber,
                string dateOfBirth = null,
                bool? demographicsOnly = null,
                bool? includeInactivePatients = null,
                Guid? parentId = null,
                CancellationToken globalToken = default)
        {
            if (globalToken.IsCancellationRequested)
            {
                return (providerFriendlyName, null, new OperationCanceledException(globalToken));
            }

            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditAndMetricBroker.LogInformationAsync(
                auditType,
                title: $"{provider.DisplayName} Provider Execution Started",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            Guid providerSpanId = await this.identifierBroker.GetIdentifierAsync();
            DateTimeOffset providerStarted = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            var providerStopwatch = Stopwatch.StartNew();
            int maxWaitTimeout = this.patientServiceConfig.MaxProviderWaitTimeMilliseconds;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(globalToken);

            if (maxWaitTimeout > 0)
            {
                timeoutCts.CancelAfter(maxWaitTimeout);
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();

                var json = await provider.Patients.GetStructuredRecordSerialisedAsync(
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients,
                    timeoutCts.Token)
                        .ConfigureAwait(false);

                stopwatch.Stop();
                long elapsedTime = stopwatch.ElapsedMilliseconds;

                // The payload is not written to the audit trail. It is persisted as a FhirRecord
                // immediately below, which is what the comparison pipeline reads, so an audit
                // copy would be a second untruncated store of the same patient bundle - one row
                // per provider per request, in a high-frequency table with no retention sweep.
                await QueueFhirRecordPersistenceAsync(
                    providerFriendlyName,
                    isPrimaryProvider,
                    provider,
                    correlationId,
                    providerSpanId,
                    auditType,
                    json);

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"{provider.DisplayName} Provider Execution Completed in {elapsedTime}ms",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());

                providerStopwatch.Stop();

                await this.auditAndMetricBroker.LogMetricAsync(new Metric
                {
                    Id = providerSpanId,
                    ParentId = parentId,
                    CorrelationId = correlationId,
                    Method = auditType,
                    Type = MetricType.Provider,

                    // The registry's friendly name, not the SPAL provider's own display name.
                    // Two rows can point at the same provider assembly - a live one and a
                    // comparison-only one - so the display name is identical for both and the
                    // spans could not be told apart. Target keeps the stable identifier.
                    Name = providerFriendlyName,
                    Target = provider.ProviderName,
                    Started = providerStarted,
                    Completed = providerStarted.AddMilliseconds(providerStopwatch.Elapsed.TotalMilliseconds),
                    DurationMs = providerStopwatch.Elapsed.TotalMilliseconds,
                    Status = MetricStatus.Succeeded,
                    PayloadBytes = json?.Length
                });

                return (providerFriendlyName, json, null);
            }
            catch (OperationCanceledException operationCancelledException)
                when (timeoutCts.IsCancellationRequested && !globalToken.IsCancellationRequested)
            {
                TimeoutException timeoutException =
                    new TimeoutException($"Provider call exceeded {maxWaitTimeout} milliseconds.",
                        operationCancelledException);

                // A provider that times out is the one worth measuring, so the span is recorded
                // on the way out rather than only on the success path.
                await RecordFailedProviderSpanAsync(
                    providerSpanId, parentId, correlationId, provider, providerFriendlyName,
                    providerStarted, providerStopwatch, MetricStatus.TimedOut, "ProviderTimeout");

                return (providerFriendlyName, null, timeoutException);
            }
            catch (OperationCanceledException operationCancelledException)
            {
                await RecordFailedProviderSpanAsync(
                    providerSpanId, parentId, correlationId, provider, providerFriendlyName,
                    providerStarted, providerStopwatch, MetricStatus.Cancelled, "ProviderCancelled");

                return (providerFriendlyName, null, operationCancelledException);
            }
            catch (Exception exception)
            {
                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Parallel Provider Execution - {provider.DisplayName} failed",

                    message:
                        $"{exception.Message} " +
                        $"{exception.InnerException?.Message} " +
                        $"{exception.InnerException?.InnerException?.Message}",

                    fileName: null,
                    correlationId: correlationId.ToString());

                await RecordFailedProviderSpanAsync(
                    providerSpanId, parentId, correlationId, provider, providerFriendlyName,
                    providerStarted, providerStopwatch, MetricStatus.Failed, exception.GetType().Name);

                return (providerFriendlyName, null, exception);
            }
        }

        /// <summary>
        /// Persists the provider's payload as a pending FhirRecord without the request waiting
        /// for the write. The record is built and stamped on the caller thread, because the
        /// audit values come from the request's identity and that identity is gone once the
        /// request scope is; the insert and its Persist span then go through the dispatcher's
        /// bounded queue, on a storage broker the factory creates fresh for exactly this reason.
        ///
        /// A refused dispatch is logged rather than thrown. Unlike a dropped metric this loses
        /// data - the record feeds later processing - but failing the patient request over a
        /// full telemetry queue would be worse, and the warning keeps the loss visible.
        /// </summary>
        private async ValueTask QueueFhirRecordPersistenceAsync(
            string providerFriendlyName,
            bool isPrimaryProvider,
            IFhirProvider provider,
            Guid correlationId,
            Guid providerSpanId,
            string auditType,
            string json)
        {
            Guid identifier = await this.identifierBroker.GetIdentifierAsync();
            string providerDisplayName = provider.DisplayName;
            string providerName = provider.ProviderName;

            FhirRecord fhirRecord = new()
            {
                Id = identifier,
                CorrelationId = correlationId.ToString(),
                JsonPayload = json,
                SourceName = $"{providerDisplayName} ({providerFriendlyName})",
                IsPrimarySource = isPrimaryProvider,
                Status = StatusType.Pending,
                IsProcessed = false,
            };

            fhirRecord = await this.securityAuditBroker.ApplyAddAuditValuesAsync(fhirRecord);

            bool accepted = this.dispatcher.TryDispatch(async token =>
            {
                DateTimeOffset persistStarted = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
                var persistStopwatch = Stopwatch.StartNew();

                // Both exits record a Persist span. Without the catch, a failed insert emitted no
                // Persist row in any status - and this is the only site that emits one - so a
                // persist failure rate was structurally zero, and the drain worker's one
                // anonymous error line was all a lost record left behind.
                //
                // Recorded through the metric broker, not written straight to storage. Going
                // direct would skip the metric service, and with it the column validation, the
                // IsEnabled kill switch, and the ActivitySource the telemetry publisher
                // subscribes to - so Persist would silently stop appearing in Application
                // Insights altogether. This does mean a shutdown that has already closed the
                // queue can refuse the span; that loss is bounded to a deployment and is now
                // counted separately as a close-refusal rather than reported as a full queue.
                async ValueTask RecordPersistSpanAsync(MetricStatus status, string errorCode)
                {
                    persistStopwatch.Stop();

                    await this.auditAndMetricBroker.LogMetricAsync(new Metric
                    {
                        Id = await this.identifierBroker.GetIdentifierAsync(),
                        ParentId = providerSpanId,
                        CorrelationId = correlationId,
                        Method = auditType,
                        Type = MetricType.Persist,
                        Name = providerFriendlyName,
                        Target = providerName,
                        Started = persistStarted,
                        Completed = persistStarted.AddMilliseconds(persistStopwatch.Elapsed.TotalMilliseconds),
                        DurationMs = persistStopwatch.Elapsed.TotalMilliseconds,
                        Status = status,
                        ErrorCode = errorCode,
                        PayloadBytes = json?.Length
                    }, token);
                }

                try
                {
                    await using IStorageBroker storageBroker =
                        await this.storageBrokerFactory.CreateStorageBrokerAsync();

                    await storageBroker.InsertFhirRecordAsync(fhirRecord);
                    await RecordPersistSpanAsync(MetricStatus.Succeeded, errorCode: null);
                }
                catch (Exception exception)
                {
                    await this.loggingBroker.LogErrorAsync(exception);

                    await this.loggingBroker.LogWarningAsync(
                        "A FHIR record was not persisted. " +
                            $"CorrelationId: {correlationId}, FhirRecordId: {fhirRecord.Id}, " +
                            $"Source: {providerDisplayName} ({providerFriendlyName}).");

                    try
                    {
                        await RecordPersistSpanAsync(MetricStatus.Failed, exception.GetType().Name);
                    }
                    catch (Exception spanException)
                    {
                        // Whatever failed the insert most likely fails this too. The warning above
                        // is already on the record; a second failure must not escape the queue.
                        await this.loggingBroker.LogErrorAsync(spanException);
                    }
                }
            });

            if (accepted is false)
            {
                await this.loggingBroker.LogWarningAsync(
                    "A FHIR record persistence was dropped because the dispatch queue was full. " +
                        $"CorrelationId: {correlationId}, Source: {providerDisplayName} " +
                        $"({providerFriendlyName}).");
            }
        }

        /// <summary>
        /// The failure exits all record the same span, differing only in how they failed. The
        /// error code is the exception type rather than its message, which keeps patient
        /// identifiable detail out of a table that is reported on and retained separately.
        /// </summary>
        private async ValueTask RecordFailedProviderSpanAsync(
            Guid providerSpanId,
            Guid? parentId,
            Guid correlationId,
            IFhirProvider provider,
            string providerFriendlyName,
            DateTimeOffset providerStarted,
            Stopwatch providerStopwatch,
            MetricStatus status,
            string errorCode)
        {
            providerStopwatch.Stop();

            await this.auditAndMetricBroker.LogMetricAsync(new Metric
            {
                Id = providerSpanId,
                ParentId = parentId,
                CorrelationId = correlationId,
                Method = "STU3-Patient-GetStructuredRecordSerialised",
                Type = MetricType.Provider,
                Name = providerFriendlyName,
                Target = provider.ProviderName,
                Started = providerStarted,
                Completed = providerStarted.AddMilliseconds(providerStopwatch.Elapsed.TotalMilliseconds),
                DurationMs = providerStopwatch.Elapsed.TotalMilliseconds,
                Status = status,
                ErrorCode = errorCode
            });
        }
    }
}
