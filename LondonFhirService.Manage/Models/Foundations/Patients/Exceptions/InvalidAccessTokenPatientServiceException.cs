// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Manage.Models.Foundations.Patients.Exceptions
{
    /// <summary>
    /// The authorisation server answered, but with something this service cannot use as a bearer
    /// token. That is the dependency's failure rather than the caller's, so it is categorised as a
    /// dependency error and not a validation one - nothing the operator typed can fix it.
    /// </summary>
    internal class InvalidAccessTokenPatientServiceException : Xeption
    {
        public InvalidAccessTokenPatientServiceException(string message)
            : base(message)
        { }
    }
}
