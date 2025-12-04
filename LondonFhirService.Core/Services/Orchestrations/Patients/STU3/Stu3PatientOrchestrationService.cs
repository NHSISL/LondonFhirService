// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.Identifiers;
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
        private readonly IAuditBroker auditBroker;
        private readonly IIdentifierBroker identityBroker;
        private readonly ILoggingBroker loggingBroker;

        public Stu3PatientOrchestrationService(
            IProviderService providerService,
            IStu3PatientService patientService,
            IStu3FhirReconciliationService fhirReconciliationService,
            IAuditBroker auditBroker,
            IIdentifierBroker identityBroker,
            ILoggingBroker loggingBroker)
        {
            this.providerService = providerService;
            this.patientService = patientService;
            this.fhirReconciliationService = fhirReconciliationService;
            this.auditBroker = auditBroker;
            this.identityBroker = identityBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Bundle> EverythingAsync(
            Guid correlationId,
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateArgsOnEverything(id, correlationId);
                string auditType = "STU3-Patient-Everything";

                string message =
                    $"Parameters:  {{ id = \"{id}\", start = \"{start}\", " +
                    $"end = \"{end}\", typeFilter = \"{typeFilter}\", " +
                    $"since = \"{since}\", count = \"{count}\" }}";

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Orchestration Service Request Submitted",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Retrieve active providers and execute request",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

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
                        provider.ActiveFrom <= now &&
                        provider.ActiveTo >= now &&
                        provider.FhirVersion == "STU3")
                    .ToList();

                ValidatePrimaryProviders(primaryProviders);
                string primaryProviderName = primaryProviders.First().Name;

                List<string> activeProviderNames = orderedProviders
                    .Where(provider =>
                        provider.IsActive &&
                        provider.ActiveFrom <= now &&
                        provider.ActiveTo >= now &&
                        provider.FhirVersion == "STU3" &&
                        !provider.IsForComparisonOnly)
                    .Select(provider => provider.Name)
                    .ToList();

                List<Bundle> bundles = await this.patientService.EverythingAsync(
                    providerNames: activeProviderNames,
                    correlationId,
                    id,
                    start,
                    end,
                    typeFilter,
                    since,
                    count,
                    cancellationToken);

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Reconcile bundles",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                Bundle reconciledBundle = await this.fhirReconciliationService.ReconcileAsync(
                    bundles: bundles,
                    primaryProviderName: primaryProviderName);

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Orchestration Service Request Completed",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                return reconciledBundle;
            });

        public ValueTask<string> EverythingSerialisedAsync(
            Guid correlationId,
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateArgsOnEverything(id, correlationId);
                string auditType = "STU3-Patient-EverythingSerialised";

                string message =
                    $"Parameters:  {{ id = \"{id}\", start = \"{start}\", " +
                    $"end = \"{end}\", typeFilter = \"{typeFilter}\", " +
                    $"since = \"{since}\", count = \"{count}\" }}";

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Orchestration Service Request Submitted",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Retrieve active providers and execute request",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

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
                        provider.ActiveFrom <= now &&
                        provider.ActiveTo >= now &&
                        provider.FhirVersion == "STU3")
                    .ToList();

                ValidatePrimaryProviders(primaryProviders);
                string primaryProviderName = primaryProviders.First().Name;

                List<string> activeProviderNames = orderedProviders
                    .Where(provider =>
                        provider.IsActive &&
                        provider.ActiveFrom <= now &&
                        provider.ActiveTo >= now &&
                        provider.FhirVersion == "STU3" &&
                        !provider.IsForComparisonOnly)
                    .Select(provider => provider.Name)
                    .ToList();

                List<string> bundles = await this.patientService.EverythingSerialisedAsync(
                    providerNames: activeProviderNames,
                    correlationId,
                    id,
                    start,
                    end,
                    typeFilter,
                    since,
                    count,
                    cancellationToken);

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Reconcile bundles",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                string reconciledBundle = await this.fhirReconciliationService.ReconcileSerialisedAsync(
                    bundles: bundles,
                    primaryProviderName: primaryProviderName);

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Orchestration Service Request Completed",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                return reconciledBundle;
            });

        public ValueTask<Bundle> GetStructuredRecordAsync(
            Guid correlationId,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateArgsOnGetStructuredRecord(nhsNumber, correlationId);
            string auditType = "STU3-Patient-GetStructuredRecord";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Submitted",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Retrieve active providers and execute request",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

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
                    provider.ActiveFrom <= now &&
                    provider.ActiveTo >= now &&
                    provider.FhirVersion == "STU3")
                .ToList();

            ValidatePrimaryProviders(primaryProviders);
            string primaryProviderName = primaryProviders.First().Name;

            List<string> activeProviderNames = orderedProviders
                .Where(provider =>
                    provider.IsActive &&
                    provider.ActiveFrom <= now &&
                    provider.ActiveTo >= now &&
                    provider.FhirVersion == "STU3" &&
                    !provider.IsForComparisonOnly)
                .Select(provider => provider.Name)
                .ToList();

            List<Bundle> bundles = await this.patientService.GetStructuredRecordAsync(
                providerNames: activeProviderNames,
                correlationId,
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Reconcile bundles",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            Bundle reconciledBundle = await this.fhirReconciliationService.ReconcileAsync(
                bundles: bundles,
                primaryProviderName: primaryProviderName);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Completed",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            return reconciledBundle;
        });

        public ValueTask<string> GetStructuredRecordSerialisedAsync(
            Guid correlationId,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            ValidateArgsOnGetStructuredRecord(nhsNumber, correlationId);
            string auditType = "STU3-Patient-GetStructuredRecord";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Submitted",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Retrieve active providers and execute request",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

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
                    provider.ActiveFrom <= now &&
                    provider.ActiveTo >= now &&
                    provider.FhirVersion == "STU3")
                .ToList();

            ValidatePrimaryProviders(primaryProviders);
            string primaryProviderName = primaryProviders.First().Name;

            List<string> activeProviderNames = orderedProviders
                .Where(provider =>
                    provider.IsActive &&
                    provider.ActiveFrom <= now &&
                    provider.ActiveTo >= now &&
                    provider.FhirVersion == "STU3" &&
                    !provider.IsForComparisonOnly)
                .Select(provider => provider.Name)
                .ToList();

            List<string> bundles = await this.patientService.GetStructuredRecordSerialisedAsync(
                providerNames: activeProviderNames,
                correlationId,
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Reconcile bundles",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            string reconciledBundle = await this.fhirReconciliationService.ReconcileSerialisedAsync(
                bundles: bundles,
                primaryProviderName: primaryProviderName);

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Completed",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            return reconciledBundle;
        });
    }
}
