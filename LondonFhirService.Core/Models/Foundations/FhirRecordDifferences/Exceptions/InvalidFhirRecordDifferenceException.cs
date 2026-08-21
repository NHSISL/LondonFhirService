// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions
{
    internal class InvalidFhirRecordDifferenceException : Xeption
    {
        public InvalidFhirRecordDifferenceException(string message)
            : base(message)
        { }
    }
}