// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net;

namespace LondonFhirService.Core.Models.Brokers.ConsumerAccesses
{
    /// <summary>
    /// What ConsumerAccessService answered, exactly as it answered it. It answers with more than
    /// one status, and uses 401 and 403 both for its answers about a consumer and for refusing this
    /// service's own credentials - told apart only by the body's media type and the errorCode on a
    /// problem. Which is which is for the service to decide, so the broker hands back everything
    /// that tells them apart untouched rather than choosing on the service's behalf.
    /// </summary>
    public class ConsumerAccessResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// The body's media type without its parameters - application/json for an Access body,
        /// application/problem+json for a problem - or null when there is no body.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The WWW-Authenticate challenge, empty when there is none. All a gateway's bare 401 has
        /// to say about why.
        /// </summary>
        public string WwwAuthenticate { get; set; }

        public string Content { get; set; }
    }
}
