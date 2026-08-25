// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    internal class FailedStorageFhirRecordException : Xeption
    {
        public FailedStorageFhirRecordException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}