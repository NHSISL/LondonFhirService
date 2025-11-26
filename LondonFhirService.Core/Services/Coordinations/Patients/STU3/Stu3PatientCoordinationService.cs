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
            await this.accessOrchestrationService.ValidateAccess(id);

            Bundle bundle = await this.patientOrchestrationService.EverythingAsync(
                id,
                start,
                end,
                typeFilter,
                since,
                count,
                cancellationToken);

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
            await this.accessOrchestrationService.ValidateAccess(id);

            string bundle = await this.patientOrchestrationService.EverythingSerialisedAsync(
                id,
                start,
                end,
                typeFilter,
                since,
                count,
                cancellationToken);

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
            await this.accessOrchestrationService.ValidateAccess(nhsNumber);

            Bundle bundle = await this.patientOrchestrationService.GetStructuredRecordAsync(
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken);

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
            await this.accessOrchestrationService.ValidateAccess(nhsNumber);

            string bundle = await this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken);

            return bundle;
        });
    }
}
