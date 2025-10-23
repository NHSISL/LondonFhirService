// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions
{
    public class ConsumerAccessValidationException : Xeption
    {
        public ConsumerAccessValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
