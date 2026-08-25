// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    internal class LockedMetricServiceException : Xeption
    {
        public LockedMetricServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
