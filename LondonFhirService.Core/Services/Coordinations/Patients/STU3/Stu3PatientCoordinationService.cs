// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
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

        public Stu3PatientCoordinationService(
            IAccessOrchestrationService accessOrchestrationService,
            IStu3PatientOrchestrationService patientOrchestrationService,
            ILoggingBroker loggingBroker)
        {
            this.accessOrchestrationService = accessOrchestrationService;
            this.patientOrchestrationService = patientOrchestrationService;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Bundle> Everything(
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
            await this.accessOrchestrationService.ValidateAccess(id);

            Bundle bundle = await this.patientOrchestrationService.Everything(
                id,
                start,
                end,
                typeFilter,
                since,
                count,
                cancellationToken);

            return bundle;
        });

        public ValueTask<Bundle> GetStructuredRecord(
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateArgsOnGetStructuredRecord(nhsNumber);
            await this.accessOrchestrationService.ValidateAccess(nhsNumber);

            Bundle bundle = await this.patientOrchestrationService.GetStructuredRecord(
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken);

            return bundle;
        });
    }
}
