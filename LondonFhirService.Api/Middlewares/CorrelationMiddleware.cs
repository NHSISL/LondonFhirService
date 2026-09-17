// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Correlations;
using Microsoft.AspNetCore.Http;

namespace LondonFhirService.Api.Middlewares
{
    /// <summary>
    /// Establishes the correlation id for the request and returns it to the consumer.
    ///
    /// It sits at the front of the pipeline so the id exists before anything can fail. A request
    /// that never reaches a controller - a 401, a 403, a timeout, a malformed body - is exactly
    /// the one a consumer needs an id for, and drawing the id deeper in meant those responses
    /// carried nothing to correlate against the logs.
    ///
    /// The id itself is resolved by the broker from W3C Trace Context - the trace the caller
    /// named in "traceparent", or the one ASP.NET generated when they named none. Nothing here
    /// parses a header: by the time this runs, the framework has already done it. The header
    /// written back is therefore the trace id in both cases, which is what lets a consumer take
    /// the value off the response and find the same request in the telemetry.
    ///
    /// It is written from OnStarting rather than inline so it lands on whatever response is
    /// eventually produced, including one written by a handler further down the pipeline, instead
    /// of only on the ones that come back through here unchanged.
    /// </summary>
    public class CorrelationMiddleware
    {
        public const string CorrelationIdHeaderName = CorrelationBroker.CorrelationIdHeaderName;

        private readonly RequestDelegate next;

        public CorrelationMiddleware(RequestDelegate next) =>
            this.next = next;

        public async Task InvokeAsync(HttpContext context, ICorrelationBroker correlationBroker)
        {
            Guid correlationId = await correlationBroker.GetCorrelationIdAsync();

            context.Response.OnStarting(() =>
            {
                // "N" - 32 hex characters, no dashes. This value IS the W3C trace id, and
                // that is the form traceparent carries and Application Insights indexes
                // operation_Id as. The default "D" form prints dashes, which matches
                // neither, so a consumer quoting the header found nothing in the viewer.
                context.Response.Headers[CorrelationIdHeaderName] = correlationId.ToString("N");

                return Task.CompletedTask;
            });

            await this.next(context);
        }
    }
}
