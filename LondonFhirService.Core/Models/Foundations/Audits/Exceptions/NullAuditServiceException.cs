// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Audits.Exceptions
{
    public class NullAuditServiceException : Xeption
    {
        public NullAuditServiceException(string message)
            : base(message)
        { }
    }
}