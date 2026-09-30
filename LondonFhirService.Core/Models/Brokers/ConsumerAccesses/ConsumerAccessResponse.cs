// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net;

namespace LondonFhirService.Core.Models.Brokers.ConsumerAccesses
{
    /// <summary>
    /// What ConsumerAccessService answered, exactly as it answered it. It answers with more than
    /// one status: 200 when access is allowed, 403 when it is refused - with the same Access body -
    /// and 401 when it does not know the consumer, with a problem details body instead. Which body
    /// shape goes with which status is for the service to decide, so the broker hands back both
    /// untouched rather than choosing on the service's behalf.
    /// </summary>
    public class ConsumerAccessResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
    }
}
