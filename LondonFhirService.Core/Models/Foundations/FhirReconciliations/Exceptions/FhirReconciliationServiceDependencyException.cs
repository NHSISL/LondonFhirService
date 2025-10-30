// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions
{
    public class FhirReconciliationServiceDependencyException : Xeption
    {
        public FhirReconciliationServiceDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}