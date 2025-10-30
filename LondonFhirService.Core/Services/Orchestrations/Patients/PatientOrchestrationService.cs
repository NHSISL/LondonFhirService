// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.FhirReconciliations;
using LondonFhirService.Core.Services.Foundations.Providers;

namespace LondonFhirService.Core.Services.Orchestrations.Patients
{
    public partial class PatientOrchestrationService : IPatientOrchestrationService
    {
        private readonly IProviderService providerService;
        private readonly IPatientService patientService;
        private readonly IFhirReconciliationService fhirReconciliationService;
        private readonly ILoggingBroker loggingBroker;

        public PatientOrchestrationService(
            IProviderService providerService,
            IPatientService patientService,
            IFhirReconciliationService fhirReconciliationService,
            ILoggingBroker loggingBroker)
        {
            this.providerService = providerService;
            this.patientService = patientService;
            this.fhirReconciliationService = fhirReconciliationService;
            this.loggingBroker = loggingBroker;
        }

        public async ValueTask<Bundle> Everything(
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Provider> allProviders =
                await this.providerService.RetrieveAllProvidersAsync();

            List<Provider> orderedProviders = allProviders
                .OrderByDescending(provider => provider.IsPrimary)
                .ToList();

            DateTimeOffset now = DateTimeOffset.UtcNow;

            List<Provider> primaryProviders = orderedProviders
                .Where(provider =>
                    provider.IsActive &&
                    provider.ActiveFrom <= now &&
                    provider.ActiveTo >= now)
                .ToList();

            // Validate primary providers here

            string primaryProviderName = primaryProviders.FirstOrDefault().Name ?? string.Empty;

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
                nhsNumber: id,
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
        }
    }
}
