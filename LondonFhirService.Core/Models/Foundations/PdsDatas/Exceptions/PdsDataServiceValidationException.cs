// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class PdsDataServiceValidationException : Xeption
    {
        public PdsDataServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}