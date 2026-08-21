// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Abstractions.Models.Audits.Exceptions
{
    public class FailedStorageAuditException : Xeption
    {
        public FailedStorageAuditException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
