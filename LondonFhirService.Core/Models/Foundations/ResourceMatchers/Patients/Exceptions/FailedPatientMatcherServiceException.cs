// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Patients.Exceptions
{
    public class FailedPatientMatcherServiceException : Xeption
    {
        public FailedPatientMatcherServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
