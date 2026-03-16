// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions
{
    public class NotFoundFhirRecordDifferenceException : Xeption
    {
        public NotFoundFhirRecordDifferenceException(string message)
            : base(message)
        { }
    }
}