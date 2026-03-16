// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    public class InvalidReferenceFhirRecordException : Xeption
    {
        public InvalidReferenceFhirRecordException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}