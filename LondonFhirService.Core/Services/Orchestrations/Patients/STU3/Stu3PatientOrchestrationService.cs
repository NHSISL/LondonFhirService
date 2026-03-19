// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IIdentifierBroker identifierBroker;
        private readonly ILoggingBroker loggingBroker;

        public Stu3PatientOrchestrationService(
            IProviderService providerService,
            IStu3PatientService patientService,
            IStu3FhirReconciliationService fhirReconciliationService,
            IAuditBroker auditBroker,
            IIdentifierBroker identifierBroker,
            ILoggingBroker loggingBroker)
        {
            this.providerService = providerService;
            this.patientService = patientService;
            this.fhirReconciliationService = fhirReconciliationService;
            this.auditBroker = auditBroker;
            this.identifierBroker = identifierBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Bundle> GetStructuredRecordAsync(
            Guid correlationId,
            string nhsNumber,
            string dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
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
                fileName: null,
                correlationId: correlationId.ToString());

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Retrieve active providers and execute request",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            Provider primaryProvider;
            List<Provider> activeProviders;
            (primaryProvider, activeProviders) = await GetProviderInfo();

            List<Bundle> bundles = await this.patientService.GetStructuredRecordAsync(
                activeProviders,
                correlationId,
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients,
                cancellationToken);

            var stopwatchReconcile = Stopwatch.StartNew();

            Bundle reconciledBundle = await this.fhirReconciliationService.ReconcileAsync(
                bundles: bundles,
                primaryProvider: primaryProvider);

            stopwatchReconcile.Stop();
            long elapsedTimeReconcile = stopwatchReconcile.ElapsedMilliseconds;

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Reconcile Bundles Completed in {elapsedTimeReconcile}ms",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            stopwatch.Stop();
            long elapsedTime = stopwatch.ElapsedMilliseconds;

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Completed in {elapsedTime}ms",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            return reconciledBundle;
        });

        public ValueTask<string> GetStructuredRecordSerialisedAsync(
            Guid correlationId,
            string nhsNumber,
            string dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            ValidateArgsOnGetStructuredRecord(nhsNumber, correlationId);
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Submitted",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Retrieve active providers and execute request",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            Provider primaryProvider;
            List<Provider> activeProviders;
            (primaryProvider, activeProviders) = await GetProviderInfo();

            List<(string Provider, string Json)> bundles = await this.patientService.GetStructuredRecordSerialisedAsync(
                activeProviders,
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
                fileName: null,
                correlationId: correlationId.ToString());

            string reconciledBundle =
                await this.fhirReconciliationService.ReconcileSerialisedAsync(
                    bundles: bundles,
                    nhsNumber: nhsNumber,
                    primaryProvider: primaryProvider);

            stopwatch.Stop();
            long elapsedTime = stopwatch.ElapsedMilliseconds;

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"Orchestration Service Request Completed in {elapsedTime}ms",
                message,
                fileName: null,
                correlationId: correlationId.ToString());

            return reconciledBundle;
        });

        private async ValueTask<(Provider primaryProvider, List<Provider> activeProvider)> GetProviderInfo()
        {
            IQueryable<Provider> allProviders =
                await this.providerService.RetrieveAllProvidersAsync();

            List<Provider> orderedProviders = allProviders
                .Where(provider => provider.FhirVersion == "STU3")
                .OrderByDescending(provider => provider.IsPrimary)
                .ToList();

            DateTimeOffset now = DateTimeOffset.UtcNow;

            List<Provider> primaryProviders = orderedProviders
                .Where(provider =>
                    provider.IsPrimary &&
                    provider.IsActive &&
                    (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                    (provider.ActiveTo == null || provider.ActiveTo >= now))
                .ToList();

            ValidatePrimaryProviders(primaryProviders);
            Provider primaryProvider = primaryProviders.First();

            List<Provider> activeProviders = orderedProviders
                .Where(provider =>
                    provider.IsActive &&
                    (provider.ActiveFrom == null || provider.ActiveFrom <= now) &&
                    (provider.ActiveTo == null || provider.ActiveTo >= now))
                .ToList();

            return (primaryProvider, activeProviders);
        }
    }
}
