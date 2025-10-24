// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Audits.Exceptions
{
    public class NotFoundAuditServiceException : Xeption
    {
        public NotFoundAuditServiceException(Guid auditId)
            : base(message: $"Couldn't find audit with auditId: {auditId}.")
        { }
    }
}