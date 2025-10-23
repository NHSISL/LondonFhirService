// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    public class FailedServiceConsumerAccessException : Xeption
    {
        public FailedServiceConsumerAccessException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
