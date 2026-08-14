// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Orchestrations.Patients;
using LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    public partial class Stu3PatientCoordinationService : IStu3PatientCoordinationService
    {
        private readonly IStu3PatientOrchestrationService patientOrchestrationService;
        private readonly IStu3FhirReconciliationService fhirReconciliationService;
        private readonly ILoggingBroker loggingBroker;
        private readonly IAuditBroker auditBroker;
        private readonly IIdentifierBroker identifierBroker;

        public Stu3PatientCoordinationService(
            IStu3PatientOrchestrationService patientOrchestrationService,
            IStu3FhirReconciliationService fhirReconciliationService,
            ILoggingBroker loggingBroker,
            IAuditBroker auditBroker,
            IIdentifierBroker identifierBroker)
        {
            this.patientOrchestrationService = patientOrchestrationService;
            this.fhirReconciliationService = fhirReconciliationService;
            this.loggingBroker = loggingBroker;
            this.auditBroker = auditBroker;
            this.identifierBroker = identifierBroker;
        }

        public ValueTask<string> GetStructuredRecordSerialisedAsync(
            string nhsNumber,
            string dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            ValidateArgsOnGetStructuredRecord(nhsNumber);
            Guid correlationId = await this.identifierBroker.GetIdentifierAsync();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Submitted",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Requesting Patient Info",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            StructuredRecordsResponse structuredRecordsResponse =
                await this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients,
                    cancellationToken);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Reconcile bundles",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            string bundle = await this.fhirReconciliationService.ReconcileSerialisedAsync(
                bundles: structuredRecordsResponse.Bundles,
                nhsNumber: nhsNumber,
                primaryProvider: structuredRecordsResponse.PrimaryProvider,
                correlationId: correlationId);

            stopwatch.Stop();
            long elapsedTime = stopwatch.ElapsedMilliseconds;

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Completed in {elapsedTime}ms",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            return bundle;
        });
    }
}
