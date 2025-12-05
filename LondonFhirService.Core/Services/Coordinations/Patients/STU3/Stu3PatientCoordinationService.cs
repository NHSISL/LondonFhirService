// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
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

        public Stu3PatientCoordinationService(
            IAccessOrchestrationService accessOrchestrationService,
            IStu3PatientOrchestrationService patientOrchestrationService,
            ILoggingBroker loggingBroker,
            IAuditBroker auditBroker,
            IIdentifierBroker identifierBroker)
        {
            this.accessOrchestrationService = accessOrchestrationService;
            this.patientOrchestrationService = patientOrchestrationService;
            this.loggingBroker = loggingBroker;
            this.auditBroker = auditBroker;
            this.identifierBroker = identifierBroker;
        }

        public ValueTask<Bundle> EverythingAsync(
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateArgsOnEverything(id);
            Guid correlationId = await this.identifierBroker.GetIdentifierAsync();
            string auditType = "STU3-Patient-Everything";

            string message =
                $"Parameters:  {{ id = \"{id}\", start = \"{start}\", " +
                $"end = \"{end}\", typeFilter = \"{typeFilter}\", " +
                $"since = \"{since}\", count = \"{count}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Submitted",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Check Access Permissions",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.accessOrchestrationService.ValidateAccess(id, correlationId);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Requesting Patient Info",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            Bundle bundle = await this.patientOrchestrationService.EverythingAsync(
                correlationId,
                id,
                start,
                end,
                typeFilter,
                since,
                count,
                cancellationToken);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Completed",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            return bundle;
        });

        public ValueTask<string> EverythingSerialisedAsync(
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateArgsOnEverything(id);

            Guid correlationId = await this.identifierBroker.GetIdentifierAsync();
            string auditType = "STU3-Patient-EverythingSerialised";

            string message =
                $"Parameters:  {{ id = \"{id}\", start = \"{start}\", " +
                $"end = \"{end}\", typeFilter = \"{typeFilter}\", " +
                $"since = \"{since}\", count = \"{count}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Submitted",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Check Access Permissions",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.accessOrchestrationService.ValidateAccess(id, correlationId);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Requesting Patient Info",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            string bundle = await this.patientOrchestrationService.EverythingSerialisedAsync(
                correlationId,
                id,
                start,
                end,
                typeFilter,
                since,
                count,
                cancellationToken);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Completed",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            return bundle;
        });


        public ValueTask<Bundle> GetStructuredRecordAsync(
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateArgsOnGetStructuredRecord(nhsNumber);

            Guid correlationId = await this.identifierBroker.GetIdentifierAsync();
            string auditType = "STU3-Patient-GetStructuredRecord";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Submitted",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Check Access Permissions",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.accessOrchestrationService.ValidateAccess(nhsNumber, correlationId);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Requesting Patient Info",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            Bundle bundle = await this.patientOrchestrationService.GetStructuredRecordAsync(
                correlationId,
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Completed",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            return bundle;
        });

        public ValueTask<string> GetStructuredRecordSerialisedAsync(
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateArgsOnGetStructuredRecord(nhsNumber);

            Guid correlationId = await this.identifierBroker.GetIdentifierAsync();
            string auditType = "STU3-Patient-GetStructuredRecord";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Submitted",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Check Access Permissions",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.accessOrchestrationService.ValidateAccess(nhsNumber, correlationId);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Requesting Patient Info",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            string bundle = await this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                correlationId,
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Coordination Service Request Completed",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            return bundle;
        });
    }
}
