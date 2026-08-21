// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    public class FailedStorageMetricException : Xeption
    {
        public FailedStorageMetricException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
