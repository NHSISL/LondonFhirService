// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions
{
    public class FhirReconciliationServiceDependencyValidationException : Xeption
    {
        public FhirReconciliationServiceDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}