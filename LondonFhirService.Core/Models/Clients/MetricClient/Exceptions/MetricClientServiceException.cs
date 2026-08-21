// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Clients.MetricClient.Exceptions
{
    public class MetricClientServiceException : Xeption
    {
        public MetricClientServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
