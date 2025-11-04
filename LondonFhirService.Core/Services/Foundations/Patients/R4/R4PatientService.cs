// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Fhirs.R4;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Providers.FHIR.R4.Abstractions;
using LondonFhirService.Providers.FHIR.R4.Abstractions.Extensions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Services.Foundations.Patients.R4
{
    public partial class R4PatientService : IR4PatientService
    {
        private readonly IR4FhirBroker fhirBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly PatientServiceConfig patientServiceConfig;

        public R4PatientService(
            IR4FhirBroker fhirBroker,
            ILoggingBroker loggingBroker,
            PatientServiceConfig patientServiceConfig)
        {
            this.fhirBroker = fhirBroker;
            this.loggingBroker = loggingBroker;
            this.patientServiceConfig = patientServiceConfig;
        }

        public ValueTask<List<Bundle>> Everything(
            List<string> providerNames,
            string id,
            CancellationToken cancellationToken,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null) =>
            TryCatch(async () =>
            {
                ValidateOnGetStructuredRecord(providerNames, id);
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
                            "Patients/$everything not supported.");

                        providers.RemoveAt(i);
                    }
                }

                var tasks = providers.Select(provider => ExecuteWithTimeoutAsync(
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

        virtual internal async Task<(Bundle Bundle, Exception Exception)> ExecuteWithTimeoutAsync(
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
                var bundle = await provider.Patients.Everything(
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

                bundle.Meta.Source = provider.Source;
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
    }
}
