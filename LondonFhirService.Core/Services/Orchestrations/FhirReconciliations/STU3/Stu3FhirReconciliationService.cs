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
    public partial class Stu3FhirReconciliationService : IStu3FhirReconciliationService
    {
        private readonly ILoggingBroker loggingBroker;

        public Stu3FhirReconciliationService(ILoggingBroker loggingBroker)
        {
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<string> ReconcileSerialisedAsync(
            List<(string Provider, string Json)> bundles,
            string nhsNumber,
            Provider primaryProvider,
            Guid correlationId) =>
        TryCatch(async () =>
        {
            var bundle = bundles.FirstOrDefault(bundle => !string.IsNullOrEmpty(bundle.Json));
            ValidateBundleIsFound(bundle, nhsNumber, correlationId);

            return bundle.Json;
        });
    }
}
