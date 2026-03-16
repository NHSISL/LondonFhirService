// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions
{
    public class FailedStorageFhirRecordDifferenceException : Xeption
    {
        public FailedStorageFhirRecordDifferenceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}