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
using LondonFhirService.Core.Models.Foundations.Patients;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Services.Foundations.Patients
{
    public partial class PatientService : IPatientService
    {
        private readonly IFhirBroker fhirBroker;
        private readonly PatientServiceConfig patientServiceConfig;

        public PatientService(
            IFhirBroker fhirBroker,
            PatientServiceConfig patientServiceConfig)
        {
            this.fhirBroker = fhirBroker;
            this.patientServiceConfig = patientServiceConfig;
        }

        public async ValueTask<List<Bundle>> GetStructuredRecord(
            List<string> providers,
            string nhsNumber,
            CancellationToken cancellationToken,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null)
        {
            ValidateOnGetStructuredRecord(providers, nhsNumber);

            var tasks = providers.Select(provider => ExecuteWithTimeoutAsync(
                provider,
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
                var aggregate = new AggregateException("One or more provider calls failed or timed out.", exceptions);
                //await LogParallelRunIssuesAsync(aggregate).ConfigureAwait(false);
            }

            return bundles;
        }

        private async Task<(Bundle Bundle, Exception Exception)> ExecuteWithTimeoutAsync(
            string providerName,
            CancellationToken globalToken,
            string nhsNumber,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null)
        {
            var everythingTask = this.fhirBroker.Patients(providerName)
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
