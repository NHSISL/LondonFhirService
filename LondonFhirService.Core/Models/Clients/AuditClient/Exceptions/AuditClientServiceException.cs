// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Clients.AuditClient.Exceptions
{
    public class AuditClientServiceException : Xeption
    {
        public AuditClientServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
