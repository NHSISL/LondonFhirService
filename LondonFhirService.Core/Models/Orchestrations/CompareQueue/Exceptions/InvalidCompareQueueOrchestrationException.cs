// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions
{
    public class InvalidCompareQueueOrchestrationException : Xeption
    {
        public InvalidCompareQueueOrchestrationException(string message)
            : base(message)
        { }
    }
}
