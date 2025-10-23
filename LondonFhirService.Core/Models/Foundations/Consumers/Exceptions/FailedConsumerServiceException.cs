// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Consumers.Exceptions
{
    public class FailedConsumerServiceException : Xeption
    {
        public FailedConsumerServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}