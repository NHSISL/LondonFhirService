// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

using Xeptions;

namespace LondonFhirService.Manage.Models.Foundations.Patients.Exceptions
{
    /// <summary>
    /// No Data dictionary is accepted. The exceptions wrapped here come from HttpClient, the JSON
    /// reader and the cancellation machinery, and their Data is not the shape Xeption's summary
    /// builder assumes - it casts every value to a list of strings. Anything worth carrying out of
    /// one of those gets a typed property, not a dictionary entry.
    /// </summary>
    internal class FailedPatientDependencyException : Xeption
    {
        public FailedPatientDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
