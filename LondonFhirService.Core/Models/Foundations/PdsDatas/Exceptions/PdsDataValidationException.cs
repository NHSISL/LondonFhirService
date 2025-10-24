// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class PdsDataValidationException : Xeption
    {
        public PdsDataValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}