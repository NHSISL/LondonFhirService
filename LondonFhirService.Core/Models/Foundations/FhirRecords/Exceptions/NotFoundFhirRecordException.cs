// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    public class NotFoundFhirRecordException : Xeption
    {
        public NotFoundFhirRecordException(string message)
            : base(message)
        { }
    }
}