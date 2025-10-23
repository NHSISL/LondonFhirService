// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    public class NullConsumerAccessException : Xeption
    {
        public NullConsumerAccessException(string message)
            : base(message)
        { }
    }
}
