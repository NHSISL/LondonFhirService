// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    public class NotFoundConsumerAccessException : Xeption
    {
        public NotFoundConsumerAccessException(string message)
            : base(message)
        { }
    }
}
