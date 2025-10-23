// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    public class LockedConsumerAccessException : Xeption
    {
        public LockedConsumerAccessException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
