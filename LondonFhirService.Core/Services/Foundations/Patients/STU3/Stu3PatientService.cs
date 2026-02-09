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
using LondonFhirService.Core.Brokers.Fhirs.STU3;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Extensions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientService : IStu3PatientService
    {
        private readonly IStu3FhirBroker fhirBroker;
        private readonly IAuditBroker auditBroker;
        private readonly IIdentifierBroker identifierBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly PatientServiceConfig patientServiceConfig;

        public Stu3PatientService(
            IStu3FhirBroker fhirBroker,
            IAuditBroker auditBroker,
            IIdentifierBroker identifierBroker,
            ILoggingBroker loggingBroker,
            PatientServiceConfig patientServiceConfig)
        {
            this.fhirBroker = fhirBroker;
            this.auditBroker = auditBroker;
            this.identifierBroker = identifierBroker;
            this.loggingBroker = loggingBroker;
            this.patientServiceConfig = patientServiceConfig;
        }

        public ValueTask<List<Bundle>> GetStructuredRecordAsync(
            List<Provider> activeProviders,
            Guid correlationId,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateOnGetStructuredRecord(activeProviders, nhsNumber, correlationId);
                string auditType = "STU3-Patient-GetStructuredRecord";

                string message =
                    $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                    $"demographicsOnly = \"{demographicsOnly}\", " +
                    $"includeInactivePatients = \"{includeInactivePatients}\" }}";

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Foundation Service Request Submitted",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                List<(string providerFriendlyName, bool isPrimaryProvider, IFhirProvider provider)> fhirProviders =
                    await GetFhirProviders(activeProviders);

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Parallel Provider Execution Started",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                var tasks = fhirProviders.Select(fhirProviders => ExecuteGetStructuredRecordWithTimeoutAsync(
                    fhirProviders.providerFriendlyName,
                    fhirProviders.isPrimaryProvider,
                    fhirProviders.provider,
                    cancellationToken,
                    correlationId,
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients)).ToArray();

                var outcomes = await Task.WhenAll(tasks).ConfigureAwait(false);

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Parallel Provider Execution Completed",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                var bundles = new List<Bundle>(outcomes.Length);
                var exceptions = new List<Exception>();

                foreach (var outcome in outcomes)
                {
                    if (outcome.Bundle is not null)
                    {
                        bundles.Add(outcome.Bundle);
                    }
                    else if (outcome.Exception is not null)
                    {
                        exceptions.Add(outcome.Exception);
                    }
                }

                if (exceptions.Count > 0)
                {
                    var aggregate = new AggregateException(
                        "One or more provider calls failed or timed out.",
                        exceptions);

                    await loggingBroker.LogErrorAsync(aggregate);
                }

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Foundation Service Request Completed",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                return bundles;
            });

        public ValueTask<List<string>> GetStructuredRecordSerialisedAsync(
            List<Provider> activeProviders,
            Guid correlationId,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateOnGetStructuredRecord(activeProviders, nhsNumber, correlationId);
                string auditType = "STU3-Patient-GetStructuredRecordSerialised";

                string message =
                    $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                    $"demographicsOnly = \"{demographicsOnly}\", " +
                    $"includeInactivePatients = \"{includeInactivePatients}\" }}";

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Foundation Service Request Submitted",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                List<(string providerFriendlyName, bool isPrimaryProvider, IFhirProvider provider)> fhirProviders =
                    await GetFhirProviders(activeProviders);

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Parallel Provider Execution Started",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                var tasks = fhirProviders.Select(fhirProviders => ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    fhirProviders.providerFriendlyName,
                    fhirProviders.isPrimaryProvider,
                    fhirProviders.provider,
                    cancellationToken,
                    correlationId,
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients)).ToArray();

                var outcomes = await Task.WhenAll(tasks).ConfigureAwait(false);

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Parallel Provider Execution Completed",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                var jsonBundles = new List<string>(outcomes.Length);
                var exceptions = new List<Exception>();

                foreach (var outcome in outcomes)
                {
                    if (outcome.Json is not null)
                    {
                        jsonBundles.Add(outcome.Json);
                    }
                    else if (outcome.Exception is not null)
                    {
                        exceptions.Add(outcome.Exception);
                    }
                }

                if (exceptions.Count > 0)
                {
                    var aggregate = new AggregateException(
                        "One or more provider calls failed or timed out.",
                        exceptions);

                    await loggingBroker.LogErrorAsync(aggregate);
                }

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"Foundation Service Request Completed",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                return jsonBundles;
            });

        private async ValueTask<List<(string providerFriendlyName, bool isPrimaryProvider, IFhirProvider provider)>>
            GetFhirProviders(List<Provider> activeProviders)
        {
            List<(string providerFriendlyName, bool isPrimaryProvider, IFhirProvider provider)> fhirProviders =
                new List<(string providerFriendlyName, bool isPrimaryProvider, IFhirProvider provider)>();

            for (int i = activeProviders.Count - 1; i >= 0; i--)
            {
                var provider = fhirBroker.FhirProviders
                    .Where(provider => provider.ProviderName == activeProviders[i].FullyQualifiedName)
                    .FirstOrDefault();

                bool isSupported;

                try
                {
                    isSupported = provider.SupportsResource("Patients", "GetStructuredRecordAsync");
                }
                catch (Exception exception)
                {
                    await loggingBroker.LogErrorAsync(exception);
                    isSupported = false;
                }

                if (!isSupported)
                {
                    await loggingBroker.LogInformationAsync($"Removing '{provider.ProviderName}': " +
                        "Patients/$GetStructuredRecord not supported.");
                }
                else
                {
                    fhirProviders.Add((activeProviders[i].FriendlyName, activeProviders[i].IsPrimary, provider));
                }
            }

            return fhirProviders;
        }

        virtual internal async Task<(Bundle Bundle, Exception Exception)> ExecuteGetStructuredRecordWithTimeoutAsync(
            string providerFriendlyName,
            bool isPrimaryProvider,
            IFhirProvider provider,
            CancellationToken globalToken,
            Guid correlationId,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null)
        {
            if (globalToken.IsCancellationRequested)
            {
                return (null, new OperationCanceledException(globalToken));
            }

            string auditType = "STU3-Patient-GetStructuredRecord";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"{provider.DisplayName} Provider Execution Started",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            int maxWaitTimeout = this.patientServiceConfig.MaxProviderWaitTimeMilliseconds;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(globalToken);

            if (maxWaitTimeout > 0)
            {
                timeoutCts.CancelAfter(maxWaitTimeout);
            }

            try
            {
                var bundle = await provider.Patients.GetStructuredRecordAsync(
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients,
                    timeoutCts.Token)
                        .ConfigureAwait(false);

                Coding coding = new Coding
                {
                    System = provider.System,
                    Code = provider.Code,
                    Display = provider.ProviderName,
                    Version = provider.FhirVersion,
                };

                bundle.Meta.Extension.Add(new Extension
                {
                    Url = "http://example.org/fhir/StructureDefinition/meta-source",
                    Value = new FhirUri(provider.Source)
                });

                bundle.Meta.Tag.Add(coding);

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"{provider.DisplayName} Provider Execution Completed",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                return (bundle, null);
            }
            catch (OperationCanceledException operationCancelledException)
                when (timeoutCts.IsCancellationRequested && !globalToken.IsCancellationRequested)
            {
                TimeoutException timeoutException =
                    new TimeoutException($"Provider call exceeded {maxWaitTimeout} milliseconds.",
                        operationCancelledException);

                return (null, timeoutException);
            }
            catch (OperationCanceledException operationCancelledException)
            {
                return (null, operationCancelledException);
            }
            catch (Exception exception)
            {
                return (null, exception);
            }
        }

        virtual internal async Task<(string Json, Exception Exception)> ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
            string providerFriendlyName,
            bool isPrimaryProvider,
            IFhirProvider provider,
            CancellationToken globalToken,
            Guid correlationId,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null)
        {
            if (globalToken.IsCancellationRequested)
            {
                return (null, new OperationCanceledException(globalToken));
            }

            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{dateOfBirth}\", " +
                $"demographicsOnly = \"{demographicsOnly}\", " +
                $"includeInactivePatients = \"{includeInactivePatients}\" }}";

            await this.auditBroker.LogInformationAsync(
                auditType,
                title: $"{provider.DisplayName} Provider Execution Started",
                message,
                fileName: string.Empty,
                correlationId: correlationId.ToString());

            int maxWaitTimeout = this.patientServiceConfig.MaxProviderWaitTimeMilliseconds;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(globalToken);

            if (maxWaitTimeout > 0)
            {
                timeoutCts.CancelAfter(maxWaitTimeout);
            }

            try
            {
                var json = await provider.Patients.GetStructuredRecordSerialisedAsync(
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients,
                    timeoutCts.Token)
                        .ConfigureAwait(false);

                // TODO: Deserialise to Bundle to add meta then serialise back to JSON

                //Coding coding = new Coding
                //{
                //    System = provider.System,
                //    Code = provider.Code,
                //    Display = provider.ProviderName,
                //    Version = provider.FhirVersion,
                //};

                //bundle.Meta.Extension.Add(new Extension
                //{
                //    Url = "http://example.org/fhir/StructureDefinition/meta-source",
                //    Value = new FhirUri(provider.Source)
                //});

                //bundle.Meta.Tag.Add(coding);

                await this.auditBroker.LogInformationAsync(
                    $"{auditType}-DATA",
                    title: $"{provider.DisplayName} - DATA",
                    json,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                await this.auditBroker.LogInformationAsync(
                    auditType,
                    title: $"{provider.DisplayName} Provider Execution Completed",
                    message,
                    fileName: string.Empty,
                    correlationId: correlationId.ToString());

                return (json, null);
            }
            catch (OperationCanceledException operationCancelledException)
                when (timeoutCts.IsCancellationRequested && !globalToken.IsCancellationRequested)
            {
                TimeoutException timeoutException =
                    new TimeoutException($"Provider call exceeded {maxWaitTimeout} milliseconds.",
                        operationCancelledException);

                return (null, timeoutException);
            }
            catch (OperationCanceledException operationCancelledException)
            {
                return (null, operationCancelledException);
            }
            catch (Exception exception)
            {
                return (null, exception);
            }
        }
    }
}
