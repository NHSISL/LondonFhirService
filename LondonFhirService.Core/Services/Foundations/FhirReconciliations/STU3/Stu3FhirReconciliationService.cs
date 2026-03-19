// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.FhirReconciliations.STU3
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
            Provider primaryProvider) =>
        TryCatch(async () =>
        {
            var bundle = bundles.FirstOrDefault(bundle => !string.IsNullOrEmpty(bundle.Json));

            if (bundle == default)
            {
                throw new ResourceNotFoundException(
                    $"NotFound:Patient resource with id = '{nhsNumber}' not found.");
            }

            return bundle.Json;
        });
    }
}
