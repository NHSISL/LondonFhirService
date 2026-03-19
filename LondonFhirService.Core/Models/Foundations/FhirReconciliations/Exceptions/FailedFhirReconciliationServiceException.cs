// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    public class FailedFhirReconciliationServiceException : Xeption
    {
        public FailedFhirReconciliationServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}