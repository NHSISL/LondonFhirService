// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions
{
    public class ComparisonOrchestrationServiceException : Xeption
    {
        public ComparisonOrchestrationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
