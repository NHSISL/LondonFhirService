// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions
{
    public class FhirReconciliationServiceValidationException : Xeption
    {
        public FhirReconciliationServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}