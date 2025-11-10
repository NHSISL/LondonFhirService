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
        /// Executes the FHIR STU3 <c>$everything</c> operation for a given
        /// <see cref="Patient"/> resource across one or more configured providers.
        /// </summary>
        /// <param name="providerNames">
        /// A list of provider names indicating which upstream FHIR sources should be
        /// queried when performing the <c>$everything</c> operation.
        /// </param>
        /// <param name="id">
        /// The logical identifier of the <see cref="Patient"/> resource on which
        /// <c>$everything</c> is invoked.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to observe cancellation while the operation is in progress.
        /// </param>
        /// <param name="start">
        /// Optional clinical start date/time used to restrict returned resources
        /// to those occurring on or after this value.
        /// </param>
        /// <param name="end">
        /// Optional clinical end date/time used to restrict returned resources
        /// to those occurring on or before this value.
        /// </param>
        /// <param name="typeFilter">
        /// Optional comma-separated list of resource types
        /// (e.g., <c>"Patient,Observation,Condition"</c>).
        /// When supplied, only resources of these types are included.
        /// </param>
        /// <param name="since">
        /// Optional timestamp used to return only resources with
        /// <c>meta.lastUpdated</c> on or after this value (incremental retrieval).
        /// </param>
        /// <param name="count">
        /// Optional maximum number of resources per page in the returned bundle(s).
        /// </param>
        /// <returns>
        /// A list of <see cref="Bundle"/> instances representing the aggregated
        /// patient record as returned by the FHIR STU3 <c>$everything</c>
        /// operation across the selected providers.
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

        /// <summary>
        /// Retrieves a patient's structured record using the GP Connect / CareConnect
        /// STU3 <c>getstructuredrecord</c> operation (invoked against
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
