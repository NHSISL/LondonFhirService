// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions
{
    public class AlreadyExistsMetricException : Xeption
    {
        public AlreadyExistsMetricException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
