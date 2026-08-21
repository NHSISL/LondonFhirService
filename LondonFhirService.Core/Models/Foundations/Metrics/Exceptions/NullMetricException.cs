// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    public class NullMetricException : Xeption
    {
        public NullMetricException(string message)
            : base(message)
        { }
    }
}
