// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    internal class InvalidFhirRecordException : Xeption
    {
        public InvalidFhirRecordException(string message)
            : base(message)
        { }
    }
}