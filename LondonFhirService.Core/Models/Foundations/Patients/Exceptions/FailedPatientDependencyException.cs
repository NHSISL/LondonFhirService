// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Patients.Exceptions
{
    internal class FailedPatientDependencyException : Xeption
    {
        public FailedPatientDependencyException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
