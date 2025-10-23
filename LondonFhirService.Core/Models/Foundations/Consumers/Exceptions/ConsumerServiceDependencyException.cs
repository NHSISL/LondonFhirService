// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Consumers.Exceptions
{
    public class ConsumerServiceDependencyException : Xeption
    {
        public ConsumerServiceDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}