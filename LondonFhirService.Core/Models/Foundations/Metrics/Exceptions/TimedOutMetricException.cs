// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    public class TimedOutMetricException : Xeption
    {
        public TimedOutMetricException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
