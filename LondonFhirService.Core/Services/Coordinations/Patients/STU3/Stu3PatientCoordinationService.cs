// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Orchestrations.Patients;
using LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;
using NHSOneLondon.AuditAndMetrics.Abstractions.Models.Metrics;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    internal partial class Stu3PatientCoordinationService : IStu3PatientCoordinationService
    {
        private readonly IStu3PatientOrchestrationService patientOrchestrationService;
        private readonly IStu3FhirReconciliationService fhirReconciliationService;
        private readonly ILoggingBroker loggingBroker;
        private readonly IAuditAndMetricBroker auditAndMetricBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly IDateTimeBroker dateTimeBroker;

        public Stu3PatientCoordinationService(
            IStu3PatientOrchestrationService patientOrchestrationService,
            IStu3FhirReconciliationService fhirReconciliationService,
            ILoggingBroker loggingBroker,
            IAuditAndMetricBroker auditAndMetricBroker,
            IIdentifierBroker identifierBroker,
            IDateTimeBroker dateTimeBroker)
        {
            this.patientOrchestrationService = patientOrchestrationService;
            this.fhirReconciliationService = fhirReconciliationService;
            this.loggingBroker = loggingBroker;
            this.auditAndMetricBroker = auditAndMetricBroker;
            this.identifierBroker = identifierBroker;
            this.dateTimeBroker = dateTimeBroker;
        }

        public ValueTask<string> GetStructuredRecordSerialisedAsync(
            Guid correlationId,
            string nhsNumber,
            string dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            // The root span. Every other span of this request is a descendant of it, so its id
            // travels down as the parent id rather than the correlation id doing double duty -
            // the correlation id says which request, this says where within it.
            //
            // Started before the cancellation check and the argument validation, not after them.
            // A request that is rejected at the door is still a request: without a root row, one
            // that arrived already cancelled or with invalid arguments would be invisible to every
            // report. Only an id and a clock reading come first - no dependency is touched before
            // the check.
            Guid requestSpanId = await this.identifierBroker.GetIdentifierAsync();
            DateTimeOffset requestStarted = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            var stopwatch = Stopwatch.StartNew();

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            // Recorded on both exits. Children are written as they complete, so a request that
            // throws part way through would otherwise leave every span already written pointing
            // at a root row that never got inserted - and a failed request is exactly the one
            // worth walking the tree for.
            async ValueTask RecordRequestSpanAsync(
                MetricStatus status,
                string errorCode,
                long? payloadBytes)
            {
                stopwatch.Stop();

                await this.auditAndMetricBroker.LogMetricAsync(new Metric
                {
                    Id = requestSpanId,
                    ParentId = null,
                    CorrelationId = correlationId,
                    Method = auditType,
                    Type = MetricType.Request,
                    Name = "GetStructuredRecordSerialised",
                    Started = requestStarted,
                    Completed = requestStarted.AddMilliseconds(stopwatch.Elapsed.TotalMilliseconds),
                    DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                    Status = status,
                    ErrorCode = errorCode,
                    PayloadBytes = payloadBytes
                });
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateArgsOnGetStructuredRecord(nhsNumber, correlationId);

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Coordination Service Request Submitted",
                    message,
                    correlationId: correlationId.ToString("N"));

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Requesting Patient Info",
                    message,
                    correlationId: correlationId.ToString("N"));

                StructuredRecordsResponse structuredRecordsResponse =
                    await this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                        correlationId,
                        nhsNumber,
                        dateOfBirth,
                        demographicsOnly,
                        includeInactivePatients,
                        parentId: requestSpanId,
                        cancellationToken);

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Reconcile bundles",
                    message,
                    correlationId: correlationId.ToString("N"));

                Guid consolidationSpanId = await this.identifierBroker.GetIdentifierAsync();

                DateTimeOffset consolidationStarted =
                    await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                var consolidationStopwatch = Stopwatch.StartNew();

                string bundle = await this.fhirReconciliationService.ReconcileSerialisedAsync(
                    bundles: structuredRecordsResponse.Bundles,
                    nhsNumber: nhsNumber,
                    primaryProvider: structuredRecordsResponse.PrimaryProvider,
                    correlationId: correlationId);

                consolidationStopwatch.Stop();

                await this.auditAndMetricBroker.LogMetricAsync(new Metric
                {
                    Id = consolidationSpanId,
                    ParentId = requestSpanId,
                    CorrelationId = correlationId,
                    Method = auditType,
                    Type = MetricType.Consolidation,
                    Name = "Reconcile bundles",
                    Started = consolidationStarted,
                    Completed = consolidationStarted.AddMilliseconds(consolidationStopwatch.Elapsed.TotalMilliseconds),
                    DurationMs = consolidationStopwatch.Elapsed.TotalMilliseconds,
                    Status = MetricStatus.Succeeded,
                    PayloadBytes = bundle?.Length,
                    Description = $"Reconciled {structuredRecordsResponse.Bundles.Count} provider bundle(s)."
                });

                await this.auditAndMetricBroker.LogInformationAsync(
                    auditType,
                    title: $"Coordination Service Request Completed in {stopwatch.ElapsedMilliseconds}ms",
                    message,
                    correlationId: correlationId.ToString("N"));

                // Last statement in the try on purpose. Anything that throws after the success
                // span is written sends the catch down the same path, and the span id is
                // inserted a second time with a contradictory status.
                await RecordRequestSpanAsync(
                    MetricStatus.Succeeded,
                    errorCode: null,
                    payloadBytes: bundle?.Length);

                return bundle;
            }
            catch (OperationCanceledException operationCanceledException)
            {
                // Classified rather than lumped in with Failed. A client abort and a genuine
                // fault produce very different durations, and MetricStatus exists so the two are
                // never averaged together - the foundation layer already stamps Cancelled and
                // TimedOut per provider, so the root span was the only place losing the
                // distinction.
                await RecordRequestSpanAsync(
                    MetricStatus.Cancelled,
                    errorCode: operationCanceledException.GetType().Name,
                    payloadBytes: null);

                throw;
            }
            catch (TimeoutException timeoutException)
            {
                await RecordRequestSpanAsync(
                    MetricStatus.TimedOut,
                    errorCode: timeoutException.GetType().Name,
                    payloadBytes: null);

                throw;
            }
            catch (Exception exception)
            {
                await RecordRequestSpanAsync(
                    ClassifyFailure(exception),
                    errorCode: exception.GetType().Name,
                    payloadBytes: null);

                throw;
            }
        });

        /// <summary>
        /// A timeout that has already been localised arrives here wrapped in a dependency
        /// exception, so the cause survives only in the inner chain. Walking it keeps the root
        /// span's status honest instead of recording every localised abort as Failed. The caller's
        /// own cancellation is never wrapped and is caught above, but a dependency outside this
        /// service's control may still bury one, so the chain is checked for it too.
        /// </summary>
        private static MetricStatus ClassifyFailure(Exception exception)
        {
            for (Exception current = exception; current is not null; current = current.InnerException)
            {
                if (current is TimeoutException)
                {
                    return MetricStatus.TimedOut;
                }

                if (current is OperationCanceledException)
                {
                    return MetricStatus.Cancelled;
                }
            }

            return MetricStatus.Failed;
        }
    }
}
