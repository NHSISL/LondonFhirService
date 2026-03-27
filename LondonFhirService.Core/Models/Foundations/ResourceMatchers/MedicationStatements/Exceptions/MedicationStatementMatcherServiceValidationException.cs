// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.MedicationStatements.Exceptions
{
    public class MedicationStatementMatcherServiceValidationException : Xeption
    {
        public MedicationStatementMatcherServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
