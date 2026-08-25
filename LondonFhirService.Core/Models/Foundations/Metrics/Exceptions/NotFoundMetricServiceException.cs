// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    internal class NotFoundMetricServiceException : Xeption
    {
        public NotFoundMetricServiceException(string message)
            : base(message)
        { }
    }
}
