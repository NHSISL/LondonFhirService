// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Audits.Exceptions
{
    public class AlreadyExistsAuditServiceException : Xeption
    {
        public AlreadyExistsAuditServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}