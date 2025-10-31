// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions
{
    public class FhirReconciliationServiceException : Xeption
    {
        public FhirReconciliationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}