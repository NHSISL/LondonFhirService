// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions
{
    internal class FailedFhirRecordDifferenceServiceException : Xeption
    {
        public FailedFhirRecordDifferenceServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}