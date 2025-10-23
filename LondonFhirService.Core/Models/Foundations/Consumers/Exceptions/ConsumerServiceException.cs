// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Consumers.Exceptions
{
    public class ConsumerServiceException : Xeption
    {
        public ConsumerServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}