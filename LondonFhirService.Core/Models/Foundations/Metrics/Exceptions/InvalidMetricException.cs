// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    public class InvalidMetricException : Xeption
    {
        public InvalidMetricException(string message)
            : base(message)
        { }
    }
}
