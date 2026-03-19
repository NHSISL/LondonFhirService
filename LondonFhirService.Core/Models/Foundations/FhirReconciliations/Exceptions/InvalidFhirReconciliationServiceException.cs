// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions
{
    public class InvalidFhirReconciliationServiceException : Xeption
    {
        public InvalidFhirReconciliationServiceException(string message)
            : base(message)
        { }
    }
}