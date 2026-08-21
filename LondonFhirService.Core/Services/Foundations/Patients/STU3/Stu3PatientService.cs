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
        private readonly PatientServiceConfig patientServiceConfig;

        public Stu3PatientService(
            IStu3FhirBroker fhirBroker,
            IAuditAndMetricBroker auditAndMetricBroker,
            IIdentifierBroker identifierBroker,
            IDateTimeBroker dateTimeBroker,
            ISecurityAuditBroker securityAuditBroker,
            IStorageBrokerFactory storageBrokerFactory,
            ILoggingBroker loggingBroker,
            PatientServiceConfig patientServiceConfig)
        {
            this.fhirBroker = fhirBroker;
            this.auditAndMetricBroker = auditAndMetricBroker;
            this.identifierBroker = identifierBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.storageBrokerFactory = storageBrokerFactory;
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
                // Provider span from it gives the time that provider's result sat idle waiting
                // for the slowest sibling.
                Guid fanOutSpanId = await this.identifierBroker.GetIdentifierAsync();
                DateTimeOffset fanOutStarted = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

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

                var stopwatchOutcomes = Stopwatch.StartNew();
                var outcomes = await Task.WhenAll(tasks).ConfigureAwait(false);
                stopwatchOutcomes.Stop();
                long elapsedTimeOutcomes = stopwatchOutcomes.ElapsedMilliseconds;

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
                    Status = MetricStatus.Succeeded,
                    Description = $"{fhirProviders.Count} provider task(s) awaited."
                });

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Parallel Provider Execution Completed in {elapsedTimeOutcomes}ms",
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

            for (int i = activeProviders.Count - 1; i >= 0; i--)
            {
                var provider = fhirBroker.FhirProviders
                    .Where(provider => provider.ProviderName == activeProviders[i].FullyQualifiedName)
                    .FirstOrDefault();

                bool isSupported;

                try
                {
                    isSupported = provider.SupportsResource("Patients", "GetStructuredRecordAsync");

                    if (!isSupported)
                    {
                        if (provider != null)
                        {
                            await loggingBroker.LogInformationAsync($"Removing '{provider.ProviderName}': " +
                                "Patients/$GetStructuredRecord not supported.");
                        }
                        else
                        {
                            await loggingBroker.LogInformationAsync(
                                $"Removing '{activeProviders[i].FriendlyName}' as " +
                                    $"'{activeProviders[i].FullyQualifiedName}' not found.");
                        }
                    }
                    else
                    {
                        fhirProviders.Add((activeProviders[i].FriendlyName, activeProviders[i].IsPrimary, provider));
                    }
                }
                catch (Exception exception)
                {
                    await loggingBroker.LogErrorAsync(exception);
                    isSupported = false;
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

                await this.auditAndMetricBroker.LogMetricAsync(new Metric
                {
                    Id = await this.identifierBroker.GetIdentifierAsync(),
                    ParentId = providerSpanId,
                    CorrelationId = correlationId,
                    Method = auditType,
                    Type = MetricType.ProviderCall,
                    Name = provider.DisplayName,
                    Target = provider.ProviderName,
                    Started = providerStarted,
                    Completed = providerStarted.AddMilliseconds(stopwatch.Elapsed.TotalMilliseconds),
                    DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                    Status = MetricStatus.Succeeded,
                    PayloadBytes = json?.Length
                });

                await this.auditAndMetricBroker.LogInformationAsync(
                    $"{auditType}-DATA",
                    title: $"{provider.DisplayName} - DATA ({providerFriendlyName})",
                    json,
                    fileName: null,
                    correlationId: correlationId.ToString());

                Guid identifier = await identifierBroker.GetIdentifierAsync();

                FhirRecord fhirRecord = new()
                {
                    Id = identifier,
                    CorrelationId = correlationId.ToString(),
                    JsonPayload = json,
                    SourceName = $"{provider.DisplayName} ({providerFriendlyName})",
                    IsPrimarySource = isPrimaryProvider,
                    Status = StatusType.Pending,
                    IsProcessed = false,
                };

                fhirRecord = await this.securityAuditBroker.ApplyAddAuditValuesAsync(fhirRecord);

                DateTimeOffset persistStarted = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
                var persistStopwatch = Stopwatch.StartNew();

                await using IStorageBroker storageBroker =
                    await this.storageBrokerFactory.CreateStorageBrokerAsync();

                await storageBroker.InsertFhirRecordAsync(fhirRecord);
                persistStopwatch.Stop();

                await this.auditAndMetricBroker.LogMetricAsync(new Metric
                {
                    Id = await this.identifierBroker.GetIdentifierAsync(),
                    ParentId = providerSpanId,
                    CorrelationId = correlationId,
                    Method = auditType,
                    Type = MetricType.Persist,
                    Name = provider.DisplayName,
                    Target = provider.ProviderName,
                    Started = persistStarted,
                    Completed = persistStarted.AddMilliseconds(persistStopwatch.Elapsed.TotalMilliseconds),
                    DurationMs = persistStopwatch.Elapsed.TotalMilliseconds,
                    Status = MetricStatus.Succeeded,
                    PayloadBytes = json?.Length
                });

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
                    Name = provider.DisplayName,
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
                Name = provider.DisplayName,
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
