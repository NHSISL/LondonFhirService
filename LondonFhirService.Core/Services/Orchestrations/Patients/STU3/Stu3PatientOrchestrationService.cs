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
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.FhirReconciliations.STU3;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using LondonFhirService.Core.Services.Foundations.Providers;

namespace LondonFhirService.Core.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationService : IStu3PatientOrchestrationService
    {
        private readonly IProviderService providerService;
        private readonly IStu3PatientService patientService;
        private readonly IStu3FhirReconciliationService fhirReconciliationService;
        private readonly ILoggingBroker loggingBroker;

        public Stu3PatientOrchestrationService(
            IProviderService providerService,
            IStu3PatientService patientService,
            IStu3FhirReconciliationService fhirReconciliationService,
            ILoggingBroker loggingBroker)
        {
            this.providerService = providerService;
            this.patientService = patientService;
            this.fhirReconciliationService = fhirReconciliationService;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Bundle> EverythingAsync(
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

                DateTimeOffset now = DateTimeOffset.UtcNow;

                List<Provider> primaryProviders = orderedProviders
                    .Where(provider =>
                        provider.IsPrimary &&
                        provider.IsActive &&
                        (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                        (provider.ActiveTo == null || provider.ActiveTo >= now) &&
                        provider.FhirVersion == "STU3")
                    .ToList();

                ValidatePrimaryProviders(primaryProviders);
                string primaryProviderName = primaryProviders.First().Name;

                List<string> activeProviderNames = orderedProviders
                    .Where(provider =>
                        provider.IsActive &&
                        (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                        (provider.ActiveTo == null || provider.ActiveTo >= now) &&
                        provider.FhirVersion == "STU3" &&
                        !provider.IsForComparisonOnly)
                    .Select(provider => provider.Name)
                    .ToList();

                List<Bundle> bundles = await this.patientService.EverythingAsync(
                    providerNames: activeProviderNames,
                    id: id,
                    cancellationToken: cancellationToken,
                    start: start,
                    end: end,
                    typeFilter: typeFilter,
                    since: since,
                    count: count);

                Bundle reconciledBundle = await this.fhirReconciliationService.ReconcileAsync(
                    bundles: bundles,
                    primaryProviderName: primaryProviderName);

                return reconciledBundle;
            });

        public ValueTask<string> EverythingSerialisedAsync(
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

                DateTimeOffset now = DateTimeOffset.UtcNow;

                List<Provider> primaryProviders = orderedProviders
                    .Where(provider =>
                        provider.IsPrimary &&
                        provider.IsActive &&
                        (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                        (provider.ActiveTo == null || provider.ActiveTo >= now) &&
                        provider.FhirVersion == "STU3")
                    .ToList();

                ValidatePrimaryProviders(primaryProviders);
                string primaryProviderName = primaryProviders.First().Name;

                List<string> activeProviderNames = orderedProviders
                    .Where(provider =>
                        provider.IsActive &&
                        (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                        (provider.ActiveTo == null || provider.ActiveTo >= now) &&
                        provider.FhirVersion == "STU3" &&
                        !provider.IsForComparisonOnly)
                    .Select(provider => provider.Name)
                    .ToList();

                List<string> bundles = await this.patientService.EverythingSerialisedAsync(
                    providerNames: activeProviderNames,
                    id: id,
                    cancellationToken: cancellationToken,
                    start: start,
                    end: end,
                    typeFilter: typeFilter,
                    since: since,
                    count: count);

                string reconciledBundle = await this.fhirReconciliationService.ReconcileSerialisedAsync(
                    bundles: bundles,
                    primaryProviderName: primaryProviderName);

                return reconciledBundle;
            });

        public ValueTask<Bundle> GetStructuredRecordAsync(
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateArgsOnGetStructuredRecord(nhsNumber);

            IQueryable<Provider> allProviders =
                await this.providerService.RetrieveAllProvidersAsync();

            List<Provider> orderedProviders = allProviders
                .OrderByDescending(provider => provider.IsPrimary)
                .ToList();

            DateTimeOffset now = DateTimeOffset.UtcNow;

            List<Provider> primaryProviders = orderedProviders
                .Where(provider =>
                    provider.IsPrimary &&
                    provider.IsActive &&
                    (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                    (provider.ActiveTo == null || provider.ActiveTo >= now) &&
                    provider.FhirVersion == "STU3")
                .ToList();

            ValidatePrimaryProviders(primaryProviders);
            string primaryProviderName = primaryProviders.First().Name;

            List<string> activeProviderNames = orderedProviders
                .Where(provider =>
                    provider.IsActive &&
                    (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                    (provider.ActiveTo == null || provider.ActiveTo >= now) &&
                    provider.FhirVersion == "STU3" &&
                    !provider.IsForComparisonOnly)
                .Select(provider => provider.Name)
                .ToList();

            List<Bundle> bundles = await this.patientService.GetStructuredRecordAsync(
                providerNames: activeProviderNames,
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken: cancellationToken);

            Bundle reconciledBundle = await this.fhirReconciliationService.ReconcileAsync(
                bundles: bundles,
                primaryProviderName: primaryProviderName);

            return reconciledBundle;
        });

        public ValueTask<string> GetStructuredRecordSerialisedAsync(
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateArgsOnGetStructuredRecord(nhsNumber);

            IQueryable<Provider> allProviders =
                await this.providerService.RetrieveAllProvidersAsync();

            List<Provider> orderedProviders = allProviders
                .OrderByDescending(provider => provider.IsPrimary)
                .ToList();

            DateTimeOffset now = DateTimeOffset.UtcNow;

            List<Provider> primaryProviders = orderedProviders
                .Where(provider =>
                    provider.IsPrimary &&
                    provider.IsActive &&
                    (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                    (provider.ActiveTo == null || provider.ActiveTo >= now) &&
                    provider.FhirVersion == "STU3")
                .ToList();

            ValidatePrimaryProviders(primaryProviders);
            string primaryProviderName = primaryProviders.First().Name;

            List<string> activeProviderNames = orderedProviders
                .Where(provider =>
                    provider.IsActive &&
                    (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                    (provider.ActiveTo == null || provider.ActiveTo >= now) &&
                    provider.FhirVersion == "STU3" &&
                    !provider.IsForComparisonOnly)
                .Select(provider => provider.Name)
                .ToList();

            List<string> bundles = await this.patientService.GetStructuredRecordSerialisedAsync(
                providerNames: activeProviderNames,
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken: cancellationToken);

            string reconciledBundle = await this.fhirReconciliationService.ReconcileSerialisedAsync(
                bundles: bundles,
                primaryProviderName: primaryProviderName);

            return reconciledBundle;
        });
    }
}
