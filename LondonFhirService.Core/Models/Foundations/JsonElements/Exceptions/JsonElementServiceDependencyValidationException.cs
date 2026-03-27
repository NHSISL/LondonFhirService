// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions
{
    public class JsonElementServiceDependencyValidationException : Xeption
    {
        public JsonElementServiceDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}