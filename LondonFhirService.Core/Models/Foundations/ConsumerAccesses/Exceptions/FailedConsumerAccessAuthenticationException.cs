// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    internal class FailedConsumerAccessAuthenticationException : Xeption
    {
        public FailedConsumerAccessAuthenticationException(string message)
            : base(message)
        { }
    }
}
