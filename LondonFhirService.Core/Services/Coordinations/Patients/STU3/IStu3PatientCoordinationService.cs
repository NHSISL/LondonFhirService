// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    public interface IStu3PatientCoordinationService
    {
        /// <summary>
        /// The correlation id is supplied by the caller rather than drawn here. The exposer
        /// already has one from the request pipeline and has returned it to the consumer in a
        /// response header, so a second id minted at this layer would split the trace in two:
        /// the one the consumer holds and the one the audit rows were written under.
        /// </summary>
        ValueTask<string> GetStructuredRecordSerialisedAsync(
            Guid correlationId,
            string nhsNumber,
            string dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default);
    }
}
