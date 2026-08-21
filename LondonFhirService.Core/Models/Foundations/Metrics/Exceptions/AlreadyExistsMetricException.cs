// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Metrics.Exceptions
{
    public class AlreadyExistsMetricException : Xeption
    {
        public AlreadyExistsMetricException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
