// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net;
using System.Net.Http;

namespace LondonFhirService.Manage.Brokers.Https
{
    /// <summary>
    /// A non-2xx answer from an upstream, carrying what that upstream said about it.
    ///
    /// It derives from HttpRequestException rather than replacing it so the status code stays
    /// where every caller already looks for it, and so a caller that only cares the call failed
    /// needs to know nothing about this type.
    ///
    /// The body travels on a property and deliberately NOT in Exception.Data. Data is the
    /// dictionary Xeption and RESTFulSense walk to build a validation summary, and both assume
    /// every value in it is a list of strings - so a raw body there is a cast failure waiting to
    /// happen. Worse, LoggingBroker appends that summary to the message it logs, which would put
    /// an OperationOutcome naming a patient into Application Insights. A property is read only by
    /// something that asks for it by name.
    /// </summary>
    public class HttpResponseException : HttpRequestException
    {
        public string ResponseBody { get; }

        public HttpResponseException(
            string message,
            HttpStatusCode statusCode,
            string responseBody)
            : base(message, inner: null, statusCode: statusCode) =>
            ResponseBody = responseBody;
    }
}
