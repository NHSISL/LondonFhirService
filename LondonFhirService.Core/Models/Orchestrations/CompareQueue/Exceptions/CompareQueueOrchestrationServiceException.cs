// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions
{
    public class CompareQueueOrchestrationServiceException : Xeption
    {
        public CompareQueueOrchestrationServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
