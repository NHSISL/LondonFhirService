// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Fhirs.STU3;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Extensions;

namespace LondonFhirService.Core.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientService : IStu3PatientService
    {
        private readonly IStu3FhirBroker fhirBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly PatientServiceConfig patientServiceConfig;

        public Stu3PatientService(
            IStu3FhirBroker fhirBroker,
            ILoggingBroker loggingBroker,
            PatientServiceConfig patientServiceConfig)
        {
            this.fhirBroker = fhirBroker;
            this.loggingBroker = loggingBroker;
            this.patientServiceConfig = patientServiceConfig;
        }

        public ValueTask<List<Bundle>> EverythingAsync(
            List<string> providerNames,
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateOnEverything(providerNames, id);
                var nameSet = new HashSet<string>(providerNames, StringComparer.OrdinalIgnoreCase);

                List<IFhirProvider> providers = fhirBroker.FhirProviders
                    .Where(provider => nameSet.Contains(provider.ProviderName)).ToList();

                for (int i = providers.Count - 1; i >= 0; i--)
                {
                    var provider = providers[i];

                    bool isSupported;

                    try
                    {
                        isSupported = provider.SupportsResource("Patients", "Everything");
                    }
                    catch (Exception exception)
                    {
                        await loggingBroker.LogErrorAsync(exception);
                        isSupported = false;
                    }

                    if (!isSupported)
                    {
                        await loggingBroker.LogInformationAsync($"Removing '{provider.ProviderName}': " +
                            "Patients/$Everything not supported.");

                        providers.RemoveAt(i);
                    }
                }

                var tasks = providers.Select(provider => ExecuteEverythingWithTimeoutAsync(
                    provider,
                    cancellationToken,
                    id,
                    start,
                    end,
                    typeFilter,
                    since,
                    count)).ToArray();

                var outcomes = await Task.WhenAll(tasks).ConfigureAwait(false);
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

                return bundles;
            });

        public ValueTask<List<string>> EverythingSerialisedAsync(
            List<string> providerNames,
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateOnEverything(providerNames, id);
                var nameSet = new HashSet<string>(providerNames, StringComparer.OrdinalIgnoreCase);

                List<IFhirProvider> providers = fhirBroker.FhirProviders
                    .Where(provider => nameSet.Contains(provider.ProviderName)).ToList();

                for (int i = providers.Count - 1; i >= 0; i--)
                {
                    var provider = providers[i];

                    bool isSupported;

                    try
                    {
                        isSupported = provider.SupportsResource("Patients", "Everything");
                    }
                    catch (Exception exception)
                    {
                        await loggingBroker.LogErrorAsync(exception);
                        isSupported = false;
                    }

                    if (!isSupported)
                    {
                        await loggingBroker.LogInformationAsync($"Removing '{provider.ProviderName}': " +
                            "Patients/$Everything not supported.");

                        providers.RemoveAt(i);
                    }
                }

                var tasks = providers.Select(provider => ExecuteEverythingSerialisedWithTimeoutAsync(
                    provider,
                    cancellationToken,
                    id,
                    start,
                    end,
                    typeFilter,
                    since,
                    count)).ToArray();

                var outcomes = await Task.WhenAll(tasks).ConfigureAwait(false);
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

                return jsonBundles;
            });

        public ValueTask<List<Bundle>> GetStructuredRecordAsync(
            List<string> providerNames,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateOnGetStructuredRecord(providerNames, nhsNumber);
                var nameSet = new HashSet<string>(providerNames, StringComparer.OrdinalIgnoreCase);

                List<IFhirProvider> providers = fhirBroker.FhirProviders
                    .Where(provider => nameSet.Contains(provider.ProviderName)).ToList();

                for (int i = providers.Count - 1; i >= 0; i--)
                {
                    var provider = providers[i];

                    bool isSupported;

                    try
                    {
                        isSupported = provider.SupportsResource("Patients", "GetStructuredRecord");
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

                        providers.RemoveAt(i);
                    }
                }

                var tasks = providers.Select(provider => ExecuteGetStructuredRecordWithTimeoutAsync(
                    provider,
                    cancellationToken,
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients)).ToArray();

                var outcomes = await Task.WhenAll(tasks).ConfigureAwait(false);
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

                return bundles;
            });

        public ValueTask<List<string>> GetStructuredRecordSerialisedAsync(
            List<string> providerNames,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateOnGetStructuredRecord(providerNames, nhsNumber);
                var nameSet = new HashSet<string>(providerNames, StringComparer.OrdinalIgnoreCase);

                List<IFhirProvider> providers = fhirBroker.FhirProviders
                    .Where(provider => nameSet.Contains(provider.ProviderName)).ToList();

                for (int i = providers.Count - 1; i >= 0; i--)
                {
                    var provider = providers[i];

                    bool isSupported;

                    try
                    {
                        isSupported = provider.SupportsResource("Patients", "GetStructuredRecord");
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

                        providers.RemoveAt(i);
                    }
                }

                var tasks = providers.Select(provider => ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    provider,
                    cancellationToken,
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients)).ToArray();

                var outcomes = await Task.WhenAll(tasks).ConfigureAwait(false);
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

                return jsonBundles;
            });

        virtual internal async Task<(Bundle Bundle, Exception Exception)> ExecuteGetStructuredRecordWithTimeoutAsync(
            IFhirProvider provider,
            CancellationToken globalToken,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null)
        {
            if (globalToken.IsCancellationRequested)
            {
                return (null, new OperationCanceledException(globalToken));
            }

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
            IFhirProvider provider,
            CancellationToken globalToken,
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null)
        {
            if (globalToken.IsCancellationRequested)
            {
                return (null, new OperationCanceledException(globalToken));
            }

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

        virtual internal async Task<(Bundle Bundle, Exception Exception)> ExecuteEverythingWithTimeoutAsync(
            IFhirProvider provider,
            CancellationToken globalToken,
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null)
        {
            if (globalToken.IsCancellationRequested)
            {
                return (null, new OperationCanceledException(globalToken));
            }

            int maxWaitTimeout = this.patientServiceConfig.MaxProviderWaitTimeMilliseconds;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(globalToken);

            if (maxWaitTimeout > 0)
            {
                timeoutCts.CancelAfter(maxWaitTimeout);
            }

            try
            {
                var bundle = await provider.Patients.EverythingAsync(
                    id,
                    start,
                    end,
                    typeFilter,
                    since,
                    count,
                    timeoutCts.Token)
                        .ConfigureAwait(false);

                Coding coding = new Coding
                {
                    System = provider.System,
                    Code = provider.Code,
                    Display = provider.ProviderName
                };

                bundle.Meta.Extension.Add(new Extension
                {
                    Url = "http://example.org/fhir/StructureDefinition/meta-source",
                    Value = new FhirUri(provider.Source)
                });

                bundle.Meta.Tag.Add(coding);

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

        virtual internal async Task<(string Json, Exception Exception)> ExecuteEverythingSerialisedWithTimeoutAsync(
            IFhirProvider provider,
            CancellationToken globalToken,
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null)
        {
            if (globalToken.IsCancellationRequested)
            {
                return (null, new OperationCanceledException(globalToken));
            }

            int maxWaitTimeout = this.patientServiceConfig.MaxProviderWaitTimeMilliseconds;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(globalToken);

            if (maxWaitTimeout > 0)
            {
                timeoutCts.CancelAfter(maxWaitTimeout);
            }

            try
            {
                string json = await provider.Patients.EverythingSerialisedAsync(
                    id,
                    start,
                    end,
                    typeFilter,
                    since,
                    count,
                    timeoutCts.Token)
                        .ConfigureAwait(false);

                // TODO: Deserialise to Bundle to add meta then serialise back to JSON
                //Coding coding = new Coding
                //{
                //    System = provider.System,
                //    Code = provider.Code,
                //    Display = provider.ProviderName
                //};

                //bundle.Meta.Extension.Add(new Extension
                //{
                //    Url = "http://example.org/fhir/StructureDefinition/meta-source",
                //    Value = new FhirUri(provider.Source)
                //});

                //bundle.Meta.Tag.Add(coding);

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
