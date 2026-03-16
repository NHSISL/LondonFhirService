// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions
{
    public class FhirRecordDifferenceDependencyValidationException : Xeption
    {
        public FhirRecordDifferenceDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}