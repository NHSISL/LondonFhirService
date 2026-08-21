// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    public class CancelledMetricException : Xeption
    {
        public CancelledMetricException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
