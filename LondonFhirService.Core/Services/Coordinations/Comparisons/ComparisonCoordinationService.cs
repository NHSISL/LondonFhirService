// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Orchestrations.Accesses;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    public class ComparisonCoordinationService : IComparisonCoordinationService
    {
        private readonly ICompareQueueOrchestrationService compareQueueOrchestrationService;
        private readonly IComparisonOrchestrationService comparisonOrchestrationService;
        private readonly ILoggingBroker loggingBroker;

        public ComparisonCoordinationService(
            ICompareQueueOrchestrationService compareQueueOrchestrationService,
            IComparisonOrchestrationService comparisonOrchestrationService,
            ILoggingBroker loggingBroker)
        {
            this.compareQueueOrchestrationService = compareQueueOrchestrationService;
            this.comparisonOrchestrationService = comparisonOrchestrationService;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask ProcessFhirRecords() =>
            throw new NotImplementedException();
    }
}
