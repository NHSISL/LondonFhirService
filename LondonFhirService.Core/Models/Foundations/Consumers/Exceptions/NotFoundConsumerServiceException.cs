// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Consumers.Exceptions
{
    public class NotFoundConsumerServiceException : Xeption
    {
        public NotFoundConsumerServiceException(string message)
            : base(message)
        { }
    }
}