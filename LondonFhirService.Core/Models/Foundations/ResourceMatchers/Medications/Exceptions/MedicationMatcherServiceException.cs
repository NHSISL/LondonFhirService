// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Medications.Exceptions
{
    public class MedicationMatcherServiceException : Xeption
    {
        public MedicationMatcherServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
