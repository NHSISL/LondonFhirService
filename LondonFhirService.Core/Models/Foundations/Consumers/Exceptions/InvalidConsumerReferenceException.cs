// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Consumers.Exceptions
{
    public class InvalidConsumerReferenceException : Xeption
    {
        public InvalidConsumerReferenceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}