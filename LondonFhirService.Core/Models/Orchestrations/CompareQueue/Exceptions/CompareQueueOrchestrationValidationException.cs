// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions
{
    public class CompareQueueOrchestrationValidationException : Xeption
    {
        public CompareQueueOrchestrationValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
