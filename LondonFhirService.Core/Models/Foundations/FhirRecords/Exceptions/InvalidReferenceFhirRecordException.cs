// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    internal class InvalidReferenceFhirRecordException : Xeption
    {
        public InvalidReferenceFhirRecordException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}