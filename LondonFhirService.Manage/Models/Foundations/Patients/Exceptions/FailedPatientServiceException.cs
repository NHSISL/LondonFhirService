// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

using Xeptions;

namespace LondonFhirService.Manage.Models.Foundations.Patients.Exceptions
{
    internal class FailedPatientServiceException : Xeption
    {
        public FailedPatientServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
