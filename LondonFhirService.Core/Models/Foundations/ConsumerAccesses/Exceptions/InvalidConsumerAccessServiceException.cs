// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    internal class InvalidConsumerAccessServiceException : Xeption
    {
        public InvalidConsumerAccessServiceException(string message)
            : base(message)
        { }
    }
}
