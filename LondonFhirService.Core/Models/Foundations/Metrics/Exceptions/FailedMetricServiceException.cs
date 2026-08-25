// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    internal class FailedMetricServiceException : Xeption
    {
        public FailedMetricServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
