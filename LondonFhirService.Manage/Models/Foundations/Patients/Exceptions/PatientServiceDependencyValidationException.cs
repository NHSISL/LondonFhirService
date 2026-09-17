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
        /// <summary>
        /// What the upstream said when it refused, or null when it said nothing this service could
        /// keep. A property rather than a Data entry on purpose: Data is what LoggingBroker turns
        /// into the text it logs, and a refusal can name a patient. This is for the operator who
        /// asked, and reaches them through the response body alone.
        /// </summary>
        public string ResponseBody { get; }

        public PatientServiceDependencyValidationException(
            string message,
            Xeption innerException,
            string responseBody = null)
            : base(message, innerException) =>
            ResponseBody = responseBody;
    }
}
