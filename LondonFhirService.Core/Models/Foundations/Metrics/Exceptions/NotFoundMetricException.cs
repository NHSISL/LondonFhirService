// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    public class NotFoundMetricException : Xeption
    {
        public NotFoundMetricException(string message)
            : base(message)
        { }
    }
}
