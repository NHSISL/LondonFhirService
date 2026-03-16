// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions
{
    public class FhirRecordDifferenceDependencyException : Xeption
    {
        public FhirRecordDifferenceDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}