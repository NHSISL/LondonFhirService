// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions;

namespace LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3
{
    internal partial class Stu3FhirReconciliationService : IStu3FhirReconciliationService
    {
        /// <summary>
        /// The NHS number is carried in the exception's data rather than its message. This
        /// exception is logged through the logging broker twice on its way out - here and again
        /// in the coordination service - and the Application Insights provider serialises the
        /// whole inner-exception chain, so an identifier in the message text lands in a telemetry
        /// store with its own retention and a broader reader set than the audit database. The
        /// correlation id in the message is what joins back to Audits, which is the store meant to
        /// hold the identifier. RESTFulSense still surfaces the data entry to the caller, who
        /// supplied the NHS number in the first place.
        /// </summary>
        private static void ValidateBundleIsFound(
            (string Provider, string Json) bundle,
            string nhsNumber,
            Guid correlationId)
        {
            if (bundle == default)
            {
                var notFoundFhirReconciliationOrchestrationException =
                    new NotFoundFhirReconciliationOrchestrationException(
                        $"NotFound:Patient resource not found.  " +
                        $"CorrelationId: {correlationId.ToString()}");

                notFoundFhirReconciliationOrchestrationException.UpsertDataList(
                    key: "nhsNumber",
                    value: nhsNumber);

                throw notFoundFhirReconciliationOrchestrationException;
            }
        }
    }
}
