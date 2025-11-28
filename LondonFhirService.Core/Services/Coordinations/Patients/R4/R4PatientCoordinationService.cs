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
using LondonFhirService.Core.Services.Orchestrations.Patients.R4;

namespace LondonFhirService.Core.Services.Coordinations.Patients.R4
{
    public partial class R4PatientCoordinationService : IR4PatientCoordinationService
    {
        private readonly IAccessOrchestrationService accessOrchestrationService;
        private readonly IR4PatientOrchestrationService patientOrchestrationService;
        private readonly ILoggingBroker loggingBroker;
        private readonly IAuditBroker auditBroker;
        private readonly IIdentifierBroker identityBroker;

        public R4PatientCoordinationService(
            IAccessOrchestrationService accessOrchestrationService,
            IR4PatientOrchestrationService patientOrchestrationService,
            ILoggingBroker loggingBroker,
            IAuditBroker auditBroker,
            IIdentifierBroker identityBroker)
        {
            this.accessOrchestrationService = accessOrchestrationService;
            this.patientOrchestrationService = patientOrchestrationService;
            this.loggingBroker = loggingBroker;
            this.auditBroker = auditBroker;
            this.identityBroker = identityBroker;
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
                Guid correlationId = await this.identityBroker.GetIdentifierAsync();
                string auditType = "R4-Patient-Everything";

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
                Guid correlationId = await this.identityBroker.GetIdentifierAsync();
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
    }
}
