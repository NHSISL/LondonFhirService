// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace LondonFhirService.Core.Services.Foundations.Patients.R4
{
    public interface IR4PatientService
    {
        /// <summary>
        /// Executes the FHIR R4 <c>$everything</c> operation for a given <see cref="Patient"/> resource.
        /// Returns a <see cref="Bundle"/> containing the patient’s consolidated health record,
        /// optionally restricted by date range, resource types, or last update time.
        /// </summary>
        /// <param name="providerNames">
        /// A list of names of the providers to be used to retrieve with <c>$everything</c>.
        /// </param>
        /// <param name="id">
        /// Logical identifier of the <see cref="Patient"/> resource on which to invoke <c>$everything</c>.
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
        /// <param name="cancellationToken">
        /// A cancellation token to allow cancelling of the request at any time.
        /// </param>
        /// <returns>
        /// A <see cref="Bundle"/> representing the aggregated patient record,
        /// as returned by the FHIR <c>$everything</c> operation.
        /// </returns>
        ValueTask<List<Bundle>> Everything(
              List<string> providerNames,
              string id,
              DateTimeOffset? start = null,
              DateTimeOffset? end = null,
              string typeFilter = null,
              DateTimeOffset? since = null,
              int? count = null,
              CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a patient's structured record using the GP Connect / CareConnect
        /// R4 <c>getstructuredrecord</c> operation (invoked against
        /// <see cref="Patient"/>), querying one or more configured FHIR providers.
        /// </summary>
        /// <remarks>
        /// Typical input values include:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <c>providerNames</c> — A list of provider identifiers specifying which FHIR
        /// systems to query.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>nhsNumber</c> — Identifier with system
        /// <c>https://fhir.hl7.org.uk/Id/nhs-number</c>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>dateOfBirth</c> (optional) — Identifier with system
        /// <c>https://fhir.hl7.org.uk/Id/dob</c>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>demographicsOnly</c> (optional) — Boolean flag restricting output to
        /// demographic information.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>includeInactivePatients</c> (optional) — Boolean flag indicating whether
        /// inactive or deceased patients should be included.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="providerNames">
        /// One or more provider names indicating which upstream FHIR systems to query.
        /// </param>
        /// <param name="nhsNumber">
        /// The patient’s NHS number (Identifier system:
        /// <c>https://fhir.hl7.org.uk/Id/nhs-number</c>).
        /// </param>
        /// <param name="dateOfBirth">
        /// Optional patient date of birth (Identifier system:
        /// <c>https://fhir.hl7.org.uk/Id/dob</c>).
        /// </param>
        /// <param name="demographicsOnly">
        /// Optional Boolean flag to request only demographic information.
        /// </param>
        /// <param name="includeInactivePatients">
        /// Optional Boolean flag indicating whether to include inactive patients.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional token to observe while awaiting the operation, defaulting to
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A list of <see cref="Bundle"/> resources containing the structured record
        /// content returned by the selected providers.
        /// </returns>
        ValueTask<List<Bundle>> GetStructuredRecord(
            List<string> providerNames,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default);
    }
}
