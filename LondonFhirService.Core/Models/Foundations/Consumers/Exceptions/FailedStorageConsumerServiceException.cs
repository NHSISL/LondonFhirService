// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Consumers.Exceptions
{
    public class FailedStorageConsumerServiceException : Xeption
    {
        public FailedStorageConsumerServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
