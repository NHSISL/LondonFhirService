// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Fhirs;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Providers.FHIR.R4.Abstractions;
using LondonFhirService.Providers.FHIR.R4.Abstractions.Extensions;
using LondonFhirService.Providers.FHIR.R4.Abstractions.Models.Resources;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Services.Foundations.Patients
{
    public partial class PatientService : IPatientService
    {
        private readonly IFhirBroker fhirBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly PatientServiceConfig patientServiceConfig;

        public PatientService(
            IFhirBroker fhirBroker,
            ILoggingBroker loggingBroker,
            PatientServiceConfig patientServiceConfig)
        {
            this.fhirBroker = fhirBroker;
            this.loggingBroker = loggingBroker;
            this.patientServiceConfig = patientServiceConfig;
        }

        public ValueTask<List<Bundle>> Everything(
            List<string> providerNames,
            string nhsNumber,
            CancellationToken cancellationToken,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null) =>
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
                        isSupported = provider.SupportsResource("Patients", "Everything");
                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine($"Provider '{p?.ProviderName}' capability check failed: {ex.Message}");
                        // Log here
                        isSupported = false;
                    }

                    if (!isSupported)
                    {
                        //Console.WriteLine($"Removing '{p?.ProviderName}': Patients/$everything not supported.");
                        // Log here
                        providers.RemoveAt(i);
                    }
                }

                var tasks = providers.Select(provider => ExecuteWithTimeoutAsync(
                    provider.Patients,
                    cancellationToken,
                    nhsNumber,
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
                }

                return bundles;
            });

        virtual internal async Task<(Bundle Bundle, Exception Exception)> ExecuteWithTimeoutAsync(
            IPatientResource resource,
            CancellationToken globalToken,
            string nhsNumber,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null)
        {
            var everythingTask = resource
                .Everything(nhsNumber, start, end, typeFilter, since, count, globalToken).AsTask();

            int maxWaitTimeout = this.patientServiceConfig.MaxProviderWaitTimeMilliseconds;
            var timeoutTask = Task.Delay(maxWaitTimeout);

            var completed = await Task.WhenAny(everythingTask, timeoutTask).ConfigureAwait(false);

            if (completed == everythingTask)
            {
                try
                {
                    var bundle = await everythingTask.ConfigureAwait(false);
                    return (bundle, null);
                }
                catch (OperationCanceledException oce)
                {
                    return (null, oce);
                }
                catch (Exception ex)
                {
                    return (null, ex);
                }
            }

            return (null, new TimeoutException($"Provider call exceeded {maxWaitTimeout}."));
        }
    }
}
