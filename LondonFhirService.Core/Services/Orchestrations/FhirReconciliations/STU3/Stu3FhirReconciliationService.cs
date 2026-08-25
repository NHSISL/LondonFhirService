// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3
{
    internal partial class Stu3FhirReconciliationService : IStu3FhirReconciliationService
    {
        private readonly ILoggingBroker loggingBroker;

        public Stu3FhirReconciliationService(ILoggingBroker loggingBroker)
        {
            this.loggingBroker = loggingBroker;
        }

        /// <summary>
        /// Returns the operator-designated primary provider's record.
        ///
        /// This used to take whichever bundle happened to be first in the list, which is not the
        /// primary's: the fan out builds its list in reverse, so positional selection returned a
        /// secondary provider's record and silently discarded the primary's. Selection is by the
        /// primary provider we were handed rather than by position, so the ordering of the list
        /// upstream can no longer change which record a consumer receives.
        ///
        /// Every bundle is still passed in. Combining them into one deduplicated record is the
        /// consolidation step this method will grow into; until then, choosing the authoritative
        /// source is the whole job.
        /// </summary>
        public ValueTask<string> ReconcileSerialisedAsync(
            List<(string Provider, string Json)> bundles,
            string nhsNumber,
            Provider primaryProvider,
            Guid correlationId) =>
        TryCatch(async () =>
        {
            string primaryProviderName = primaryProvider?.FriendlyName;

            (string Provider, string Json) bundle = bundles.FirstOrDefault(bundle =>
                string.IsNullOrEmpty(bundle.Json) == false
                    && bundle.Provider == primaryProviderName);

            if (bundle == default)
            {
                // The primary returned nothing - it failed, timed out, or held no record for this
                // patient. A secondary's record is still worth returning, but substituting one
                // silently is the defect this method just stopped committing, so it is logged.
                bundle = bundles.FirstOrDefault(bundle => string.IsNullOrEmpty(bundle.Json) == false);

                if (bundle != default)
                {
                    await this.loggingBroker.LogWarningAsync(
                        $"Primary provider '{primaryProviderName}' returned no record; " +
                            $"returning '{bundle.Provider}' instead.  " +
                            $"CorrelationId: {correlationId.ToString()}");
                }
            }

            ValidateBundleIsFound(bundle, nhsNumber, correlationId);

            return bundle.Json;
        });
    }
}
