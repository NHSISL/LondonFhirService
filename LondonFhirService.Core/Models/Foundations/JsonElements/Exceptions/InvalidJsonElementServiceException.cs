// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions
{
    public class InvalidJsonElementServiceException : Xeption
    {
        public InvalidJsonElementServiceException(string message)
            : base(message)
        { }
    }
}