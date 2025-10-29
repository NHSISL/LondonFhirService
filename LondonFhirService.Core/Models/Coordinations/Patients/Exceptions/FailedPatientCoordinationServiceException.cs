// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Coordinations.Patients.Exceptions
{
    internal class FailedPatientCoordinationServiceException : Xeption
    {
        public FailedPatientCoordinationServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}