// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions
{
    public class InvalidFhirRecordDifferenceException : Xeption
    {
        public InvalidFhirRecordDifferenceException(string message)
            : base(message)
        { }
    }
}