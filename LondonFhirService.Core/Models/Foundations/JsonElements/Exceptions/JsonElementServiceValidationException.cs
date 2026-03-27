// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions
{
    public class JsonElementServiceValidationException : Xeption
    {
        public JsonElementServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}