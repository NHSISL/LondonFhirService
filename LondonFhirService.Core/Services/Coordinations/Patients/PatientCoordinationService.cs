// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Orchestrations.Patients;

namespace LondonFhirService.Core.Services.Coordinations.Patients
{
    public partial class PatientCoordinationService : IPatientCoordinationService
    {
        private readonly IAccessOrchestrationService accessOrchestrationService;
        private readonly IPatientOrchestrationService patientOrchestrationService;
        private readonly ILoggingBroker loggingBroker;

        public PatientCoordinationService(
            IAccessOrchestrationService accessOrchestrationService,
            IPatientOrchestrationService patientOrchestrationService,
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
    }
}
