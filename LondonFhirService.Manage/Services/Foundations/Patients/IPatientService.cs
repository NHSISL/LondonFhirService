// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using LondonFhirService.Manage.Models.Foundations.Patients;

namespace LondonFhirService.Manage.Services.Foundations.Patients
{
    public interface IPatientService
    {
        /// <summary>
        /// Exchanges the request's credentials for a bearer token, then calls
        /// $getstructuredrecord with it. The provider's response body is returned as it arrived -
        /// the page renders the payload, so reformatting it here would hide what the endpoint
        /// actually answered.
        ///
        /// The correlation id the Api filed the call under comes back with it, so the page can
        /// send an operator on to the comparisons that same call produced.
        /// </summary>
        ValueTask<StructuredRecordResponse> GetStructuredRecordAsync(
            StructuredRecordRequest structuredRecordRequest,
            CancellationToken cancellationToken = default);
    }
}
