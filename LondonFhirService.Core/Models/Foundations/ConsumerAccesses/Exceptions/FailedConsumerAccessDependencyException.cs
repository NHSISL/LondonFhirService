// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    public class FailedConsumerAccessDependencyException : Xeption
    {
        public FailedConsumerAccessDependencyException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
