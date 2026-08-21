// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Orchestrations.Patients;

namespace LondonFhirService.Core.Services.Orchestrations.Patients.STU3
{
    public interface IStu3PatientOrchestrationService
    {
        ValueTask<StructuredRecordsResponse> GetStructuredRecordSerialisedAsync(
            Guid correlationId,
            string nhsNumber,
            string dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default);

        ValueTask ValidateAccess(
            string nhsNumber,
            Guid correlationId,
            CancellationToken cancellationToken = default);
    }
}
