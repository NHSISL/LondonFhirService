// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions
{
    public class ForbiddenAccessOrchestrationException : Xeption
    {
        public ForbiddenAccessOrchestrationException(string message)
            : base(message)
        { }
    }
}
