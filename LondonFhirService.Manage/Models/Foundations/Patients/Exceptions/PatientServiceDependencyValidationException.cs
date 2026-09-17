// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Manage.Models.Foundations.Patients.Exceptions
{
    /// <summary>
    /// The upstream refused for a reason the caller can act on - credentials it did not accept, a
    /// patient it does not hold. Separate from PatientServiceDependencyException, which is for the
    /// upstream failing rather than judging, because the two deserve different answers: one is the
    /// operator's to fix, the other is not.
    /// </summary>
    public class PatientServiceDependencyValidationException : Xeption
    {
        public PatientServiceDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
