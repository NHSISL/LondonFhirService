// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
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

        public R4PatientCoordinationService(
            IAccessOrchestrationService accessOrchestrationService,
            IR4PatientOrchestrationService patientOrchestrationService,
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
    }
}
