// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions
{
    internal class NotFoundFhirRecordDifferenceException : Xeption
    {
        public NotFoundFhirRecordDifferenceException(string message)
            : base(message)
        { }
    }
}