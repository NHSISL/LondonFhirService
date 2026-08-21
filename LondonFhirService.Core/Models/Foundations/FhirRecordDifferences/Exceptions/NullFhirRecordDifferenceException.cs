// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions
{
    internal class NullFhirRecordDifferenceException : Xeption
    {
        public NullFhirRecordDifferenceException(string message)
            : base(message)
        { }
    }
}