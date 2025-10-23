// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    public class FailedStorageConsumerAccessException : Xeption
    {
        public FailedStorageConsumerAccessException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
