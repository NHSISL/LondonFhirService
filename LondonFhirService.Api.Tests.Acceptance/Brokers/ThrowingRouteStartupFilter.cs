// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    /// <summary>
    /// A route that throws, so an acceptance run can exercise the one response class the host
    /// cannot otherwise produce on demand: an exception that escapes MVC entirely.
    ///
    /// It matters because that path used to come back bare. An unhandled exception reaches
    /// Kestrel, which resets the response headers and synthesises a 500 without firing the
    /// Response.OnStarting callbacks - so the correlation header, written from one of those
    /// callbacks, was missing from exactly the responses a consumer would ring up about. The
    /// exception handler in ConfigurePipeline exists to turn that into an ordinary response, and
    /// nothing but a real request through the real pipeline proves it still does.
    ///
    /// The middleware is appended after the host's own pipeline rather than in front of it, so it
    /// sits where an unmatched request lands - behind the correlation middleware and behind the
    /// exception handler, which is the arrangement under test. Requests for anything else fall
    /// through untouched.
    /// </summary>
    public class ThrowingRouteStartupFilter : IStartupFilter
    {
        public const string ThrowingRoute = "/test-only/throw";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            applicationBuilder =>
            {
                next(applicationBuilder);

                applicationBuilder.Use(async (context, nextMiddleware) =>
                {
                    if (context.Request.Path.StartsWithSegments(ThrowingRoute))
                    {
                        throw new InvalidOperationException(
                            "Deliberate unhandled exception raised by the acceptance suite.");
                    }

                    await nextMiddleware(context);
                });
            };
    }
}
