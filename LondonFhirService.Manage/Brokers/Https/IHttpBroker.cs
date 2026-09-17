// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LondonFhirService.Manage.Brokers.Https
{
    /// <summary>
    /// A thin wrapper over HTTP. Named for the transport rather than for either endpoint it is
    /// pointed at, so the same broker serves the token call and the structured record call - the
    /// urls are the caller's, not the broker's.
    ///
    /// Both methods return the raw response body as a string. Neither parses it: the token
    /// response and the FHIR bundle are the service's to read, and a broker that deserialised
    /// them would be deciding what they mean.
    ///
    /// A non-2xx response throws HttpRequestException carrying the status code, with the response
    /// body under HttpBroker.ResponseBodyKey in Exception.Data rather than being discarded. What
    /// an upstream says when it refuses is the most useful thing it ever says - but it can also
    /// name a patient, so it is kept out of the exception message, which is the part that reaches
    /// a log sink without anyone asking.
    /// </summary>
    public interface IHttpBroker
    {
        ValueTask<string> PostFormUrlEncodedContentAsync(
            string url,
            IDictionary<string, string> formValues,
            CancellationToken cancellationToken = default);

        /// <param name="mediaType">
        /// The Content-Type to send the body as. The caller supplies it because the broker has no
        /// business knowing that one endpoint speaks FHIR and another speaks plain JSON.
        /// </param>
        ValueTask<string> PostJsonContentAsync(
            string url,
            string jsonContent,
            string mediaType,
            string bearerToken,
            CancellationToken cancellationToken = default);
    }
}
