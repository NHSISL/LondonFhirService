// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    public class AlreadyExistsFhirRecordException : Xeption
    {
        public AlreadyExistsFhirRecordException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}