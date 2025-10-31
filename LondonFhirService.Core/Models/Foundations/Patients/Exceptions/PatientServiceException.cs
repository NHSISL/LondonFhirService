// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Patients.Exceptions
{
    public class PatientServiceException : Xeption
    {
        public PatientServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
