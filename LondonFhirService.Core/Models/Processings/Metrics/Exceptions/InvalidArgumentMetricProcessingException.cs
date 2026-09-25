// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.Metrics.Exceptions
{
    internal class InvalidArgumentMetricProcessingException : Xeption
    {
        public InvalidArgumentMetricProcessingException(string message)
            : base(message)
        { }
    }
}
