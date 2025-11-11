// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.FhirReconciliations.R4;
using LondonFhirService.Core.Services.Foundations.Patients.R4;
using LondonFhirService.Core.Services.Foundations.Providers;

namespace LondonFhirService.Core.Services.Orchestrations.Patients.R4
{
    public partial class R4PatientOrchestrationService : IR4PatientOrchestrationService
    {
        private readonly IProviderService providerService;
        private readonly IR4PatientService patientService;
        private readonly IR4FhirReconciliationService fhirReconciliationService;
        private readonly ILoggingBroker loggingBroker;
        private readonly IDateTimeBroker dateTimeBroker;

        public R4PatientOrchestrationService(
            IProviderService providerService,
            IR4PatientService patientService,
            IR4FhirReconciliationService fhirReconciliationService,
            ILoggingBroker loggingBroker,
            IDateTimeBroker dateTimeBroker)
        {
            this.providerService = providerService;
            this.patientService = patientService;
            this.fhirReconciliationService = fhirReconciliationService;
            this.loggingBroker = loggingBroker;
            this.dateTimeBroker = dateTimeBroker;
        }

        public ValueTask<Bundle> Everything(
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateArgsOnEverything(id);

                IQueryable<Provider> allProviders =
                    await this.providerService.RetrieveAllProvidersAsync();

                List<Provider> orderedProviders = allProviders
                    .OrderByDescending(provider => provider.IsPrimary)
                    .ToList();

                DateTimeOffset now = await dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                List<Provider> primaryProviders = orderedProviders
                    .Where(provider =>
                        provider.IsPrimary &&
                        provider.IsActive &&
                        provider.ActiveFrom <= now &&
                        provider.ActiveTo >= now)
                    .ToList();

                ValidatePrimaryProviders(primaryProviders);
                string primaryProviderName = primaryProviders.First().Name;

                List<string> activeProviderNames = orderedProviders
                    .Where(provider =>
                        provider.IsActive &&
                        provider.ActiveFrom <= now &&
                        provider.ActiveTo >= now &&
                        !provider.IsForComparisonOnly)
                    .Select(provider => provider.Name)
                    .ToList();

                List<Bundle> bundles = await this.patientService.Everything(
                    providerNames: activeProviderNames,
                    id: id,
                    cancellationToken: cancellationToken,
                    start: start,
                    end: end,
                    typeFilter: typeFilter,
                    since: since,
                    count: count);

                Bundle reconciledBundle = await this.fhirReconciliationService.Reconcile(
                    bundles: bundles,
                    primaryProviderName: primaryProviderName);

                return reconciledBundle;
            });
    }
}
