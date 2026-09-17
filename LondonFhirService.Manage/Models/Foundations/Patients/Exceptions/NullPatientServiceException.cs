// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Manage.Models.Foundations.Patients.Exceptions
{
    internal class NullPatientServiceException : Xeption
    {
        public NullPatientServiceException(string message)
            : base(message)
        { }
    }
}
