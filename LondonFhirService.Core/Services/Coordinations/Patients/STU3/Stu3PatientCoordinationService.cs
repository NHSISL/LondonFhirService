// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    public partial class Stu3PatientCoordinationService : IStu3PatientCoordinationService
    {
        private readonly IAccessOrchestrationService accessOrchestrationService;
        private readonly IStu3PatientOrchestrationService patientOrchestrationService;
        private readonly ILoggingBroker loggingBroker;
        private readonly IAuditBroker auditBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly AccessConfigurations accessConfigurations;

        public Stu3PatientCoordinationService(
            IAccessOrchestrationService accessOrchestrationService,
            IStu3PatientOrchestrationService patientOrchestrationService,
            ILoggingBroker loggingBroker,
            IAuditBroker auditBroker,
            IIdentifierBroker identifierBroker,
            AccessConfigurations accessConfigurations)
        {
            this.accessOrchestrationService = accessOrchestrationService;
            this.patientOrchestrationService = patientOrchestrationService;
            this.loggingBroker = loggingBroker;
            this.auditBroker = auditBroker;
            this.identifierBroker = identifierBroker;
            this.accessConfigurations = accessConfigurations;
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

            if (this.accessConfigurations.CheckAccessPermissions)
            {
                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Check Access Permissions",
                    message,
                    fileName: null,
                    correlationId: correlationId.ToString());

                await this.accessOrchestrationService.ValidateAccess(nhsNumber, correlationId);
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

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Requesting Patient Info",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            string bundle = await this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
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
                title: $"Coordination Service Request Completed in {elapsedTime}ms",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            return bundle;
        });
    }
}
