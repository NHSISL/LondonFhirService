// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Core.Models.Brokers.ConsumerAccesses
{
    public class ValidateAccessRequest
    {
        public string ConsumerUserId { get; set; }
        public string NhsNumber { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
