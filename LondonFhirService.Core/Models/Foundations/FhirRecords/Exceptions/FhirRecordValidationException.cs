// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    public class FhirRecordValidationException : Xeption
    {
        public FhirRecordValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}