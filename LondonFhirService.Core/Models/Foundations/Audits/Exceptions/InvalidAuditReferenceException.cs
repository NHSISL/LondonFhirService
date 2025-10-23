// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Audits.Exceptions
{
    public class InvalidAuditReferenceException : Xeption
    {
        public InvalidAuditReferenceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}