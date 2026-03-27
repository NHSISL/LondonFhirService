// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.MedicationStatements.Exceptions
{
    public class MedicationStatementMatcherServiceException : Xeption
    {
        public MedicationStatementMatcherServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
