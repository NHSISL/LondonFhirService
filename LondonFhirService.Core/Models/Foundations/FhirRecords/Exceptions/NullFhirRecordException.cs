// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    public class NullFhirRecordException : Xeption
    {
        public NullFhirRecordException(string message)
            : base(message)
        { }
    }
}