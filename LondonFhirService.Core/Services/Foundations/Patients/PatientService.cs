// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Fhirs;
using LondonFhirService.Core.Models.Foundations.Patients;

namespace LondonFhirService.Core.Services.Foundations.Patients
{
    public class PatientService : IPatientService
    {
        private readonly IFhirBroker fhirBroker;
        private readonly PatientServiceConfig patientServiceConfig;

        public PatientService(
            IFhirBroker fhirBroker,
            PatientServiceConfig patientServiceConfig)
        {
            this.fhirBroker = fhirBroker;
            this.patientServiceConfig = patientServiceConfig;
        }

        public async ValueTask<List<Bundle>> GetStructuredRecord(
            List<string> providers,
            string nhsNumber,
            CancellationToken? cancellationToken,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null)
        {
            // validate providers list and nhsNumber

            List<Bundle> bundles = new List<Bundle>();

            foreach (string provider in providers)
            {
                Bundle bundle =
                    this.fhirBroker.Patients(provider).Everything(nhsNumber, start, end, typeFilter, since, count);

                bundles.Add(bundle);
            }

            return bundles;
        }
    }
}
