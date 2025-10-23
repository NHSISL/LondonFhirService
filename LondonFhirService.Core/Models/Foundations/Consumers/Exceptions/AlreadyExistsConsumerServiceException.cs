// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Consumers.Exceptions
{
    public class AlreadyExistsConsumerServiceException : Xeption
    {
        public AlreadyExistsConsumerServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
