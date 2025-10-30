// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Coordinations.Patients.Exceptions
{
    public class FailedPatientCoordinationException : Xeption
    {
        public FailedPatientCoordinationException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}