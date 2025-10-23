// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    public class InvalidConsumerAccessException : Xeption
    {
        public InvalidConsumerAccessException(string message)
            : base(message)
        { }
    }
}
