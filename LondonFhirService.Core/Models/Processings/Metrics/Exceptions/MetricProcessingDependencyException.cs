// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.Metrics.Exceptions
{
    public class MetricProcessingDependencyException : Xeption
    {
        public MetricProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
