// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    internal class UnauthorizedConsumerAccessServiceException : Xeption
    {
        public UnauthorizedConsumerAccessServiceException(string message)
            : base(message)
        { }
    }
}
