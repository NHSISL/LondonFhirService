// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Manage.Models.Foundations.Patients.Exceptions
{
    internal class TimedOutPatientServiceException : Xeption
    {
        public TimedOutPatientServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
