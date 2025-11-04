// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace LondonFhirService.Core.Services.Foundations.Patients.STU3
{
    public interface IStu3PatientService
    {
        /// <summary>
        /// Executes the FHIR STU3 <c>$everything</c> operation for a given <see cref="Patient"/> resource.
        /// Returns a <see cref="Bundle"/> containing the patient’s consolidated health record,
        /// optionally restricted by date range, resource types, or last update time.
        /// </summary>
        /// <param name="providerNames">
        /// A list of names of the providers to be used to retrieve with <c>$everything</c>.
        /// </param>
        /// <param name="id">
        /// Logical identifier of the <see cref="Patient"/> resource on which to invoke <c>$everything</c>.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to allow cancelling of the request at any time.
        /// </param>
        /// <param name="start">
        /// Optional start date/time that constrains the resources to those with a clinical date
        /// on or after this value (e.g., encounter period, observation effective).
        /// </param>
        /// <param name="end">
        /// Optional end date/time that constrains the resources to those with a clinical date
        /// on or before this value.
        /// </param>
        /// <param name="typeFilter">
        /// Optional comma-separated list of resource types (e.g., "Patient,Observation,Condition").
        /// When specified, only resources of these types will be returned in the <see cref="Bundle"/>.
        /// </param>
        /// <param name="since">
        /// Optional timestamp used to restrict results to resources with
        /// <c>meta.lastUpdated</c> on or after this value (incremental retrieval).
        /// </param>
        /// <param name="count">
        /// Optional maximum number of resources to include per page of results.
        /// Used for paging the returned <see cref="Bundle"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Bundle"/> representing the aggregated patient record,
        /// as returned by the FHIR <c>$everything</c> operation.
        /// </returns>
        ValueTask<List<Bundle>> Everything(
              List<string> providerNames,
              string id,
              CancellationToken cancellationToken,
              DateTimeOffset? start = null,
              DateTimeOffset? end = null,
              string typeFilter = null,
              DateTimeOffset? since = null,
              int? count = null);
    }
}
