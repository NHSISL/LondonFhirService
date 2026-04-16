// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Services.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Services.Foundations.FhirRecords;

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public class CompareQueueOrchestrationService : ICompareQueueOrchestrationService
    {
        private readonly IFhirRecordService fhirRecordService;
        private readonly IFhirRecordDifferenceService fhirRecordDifferenceService;
        private readonly ILoggingBroker loggingBroker;

        public CompareQueueOrchestrationService(
            IFhirRecordService fhirRecordService,
            IFhirRecordDifferenceService fhirRecordDifferenceService,
            ILoggingBroker loggingBroker)
        {
            this.fhirRecordService = fhirRecordService;
            this.fhirRecordDifferenceService = fhirRecordDifferenceService;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<CompareQueueItems> GetUnprocessedRecordAsync() =>
            throw new NotImplementedException();

        public ValueTask MarkFhirRecordsAsync(Guid fhirRecordId) =>
            throw new NotImplementedException();

        public ValueTask PersistFhirRecordDifferencesAsync(CompareQueueItems compareQueueItems) =>
            throw new NotImplementedException();
    }
}
