// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Consumers.Exceptions
{
    public class NullConsumerServiceException : Xeption
    {
        public NullConsumerServiceException(string message)
            : base(message)
        { }
    }
}