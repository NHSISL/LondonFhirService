// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    public class InvalidFhirRecordException : Xeption
    {
        public InvalidFhirRecordException(string message)
            : base(message)
        { }
    }
}