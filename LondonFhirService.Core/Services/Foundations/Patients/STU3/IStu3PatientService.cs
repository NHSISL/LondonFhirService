// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.Patients.STU3
{
    public interface IStu3PatientService
    {
        ValueTask<List<Bundle>> GetStructuredRecordAsync(
            List<Provider> activeProviders,
            Guid correlationId,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default);

        ValueTask<List<string>> GetStructuredRecordSerialisedAsync(
            List<Provider> activeProviders,
            Guid correlationId,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default);
    }
}
