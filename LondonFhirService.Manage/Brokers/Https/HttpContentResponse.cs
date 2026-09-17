// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Manage.Brokers.Https
{
    /// <summary>
    /// A successful response's body together with the correlation id the upstream filed it under.
    ///
    /// The id is here rather than left on the HttpResponseMessage because the message is disposed
    /// before this broker returns - reading it later would mean keeping the response alive up
    /// through the service, which is how a pooled connection ends up held open by a page.
    ///
    /// CorrelationId is empty when the upstream sent no header. That is not a failure: only the
    /// hosts in this solution set X-Correlation-Id, and an authorisation server has never heard
    /// of it.
    /// </summary>
    public record HttpContentResponse(string Body, string CorrelationId);
}
