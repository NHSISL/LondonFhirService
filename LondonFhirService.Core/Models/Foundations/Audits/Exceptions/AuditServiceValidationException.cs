// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Audits.Exceptions
{
    public class AuditServiceValidationException : Xeption
    {
        public AuditServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}