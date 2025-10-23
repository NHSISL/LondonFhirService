// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Audits.Exceptions
{
    public class FailedAuditStorageException : Xeption
    {
        public FailedAuditStorageException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}