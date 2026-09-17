// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using LondonFhirService.Api.Middlewares;
using LondonFhirService.Api.Tests.Acceptance.Brokers;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Acceptance.Apis.Correlations
{
    /// <summary>
    /// That the middleware is wired in, and wired in early enough. The unit tests prove it writes
    /// the header when asked; only a run through the real pipeline proves it is asked - that
    /// responses which never reach a controller carry the header too, and that a caller's
    /// traceparent survives the whole way through ASP.NET's own parsing into our response.
    /// </summary>
    [Collection(nameof(ApiTestCollection))]
    public class CorrelationTests : IDisposable
    {
        private readonly ApiBroker apiBroker;
        private readonly ActivityListener activityListener;

        public CorrelationTests(ApiBroker apiBroker)
        {
            this.apiBroker = apiBroker;

            // The deployed host has Application Insights listening, which is what makes ASP.NET
            // create an Activity per request and parse the caller's traceparent into it. The test
            // host excludes App Insights, so without a listener of our own there is no activity
            // to read and this would be testing the fallback rather than the convention.
            this.activityListener = new ActivityListener
            {
                ShouldListenTo = _ => true,

                Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                    ActivitySamplingResult.AllDataAndRecorded
            };

            ActivitySource.AddActivityListener(this.activityListener);
        }

        public void Dispose() =>
            this.activityListener.Dispose();

        [Fact]
        public async Task ShouldReturnACorrelationIdOnASuccessfulResponseAsync()
        {
            // given . when
            HttpResponseMessage response = await this.apiBroker.GetResponseAsync("/");

            // then
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            Guid correlationId = ReadCorrelationId(response);
            correlationId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task ShouldReturnACorrelationIdOnAResponseThatNeverReachesAControllerAsync()
        {
            // given . when
            HttpResponseMessage response =
                await this.apiBroker.GetResponseAsync($"/{Guid.NewGuid()}");

            // then
            // The whole reason the id is settled in the pipeline. A consumer ringing up about a
            // request that failed needs something to quote, and a response produced before any
            // controller ran used to carry nothing at all.
            response.IsSuccessStatusCode.Should().BeFalse();
            Guid correlationId = ReadCorrelationId(response);
            correlationId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task ShouldReturnACorrelationIdOnAnUnhandledExceptionAsync()
        {
            // given . when
            HttpResponseMessage response =
                await this.apiBroker.GetResponseAsync(ThrowingRouteStartupFilter.ThrowingRoute);

            // then
            // The response class this whole arrangement exists for, and the one that cannot be
            // taken on trust. An exception that escapes MVC reaches Kestrel, which resets the
            // response headers and synthesises a 500 without firing Response.OnStarting - and the
            // header is written from an OnStarting callback, so it came back bare until the
            // exception handler was put behind the correlation middleware to turn the failure
            // into an ordinary response. Remove that handler and only this test notices.
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            Guid correlationId = ReadCorrelationId(response);
            correlationId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task ShouldReturnACorrelationIdOnAnAuthorizationRefusalAsync()
        {
            // given . when
            HttpResponseMessage response =
                await this.apiBroker.GetResponseAsync(ForbiddenProbeController.Route);

            // then
            // The response class the middleware's POSITION is for. An unmatched URL is refused by
            // routing and an unhandled exception by the error handler; neither sits behind
            // UseAuthentication, UseAuthorization or UseRequestTimeouts. This one does: the
            // authorization middleware writes it, and it only carries the header because the
            // correlation middleware ran first. Move UseMiddleware<CorrelationMiddleware> below
            // UseAuthorization and every other correlation test still passes - this one fails.
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            Guid correlationId = ReadCorrelationId(response);
            correlationId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task ShouldAdoptTheTraceTheCallerSentAsync()
        {
            // given
            string callersTraceIdHexString = ActivityTraceId.CreateRandom().ToHexString();
            Guid expectedCorrelationId = Guid.ParseExact(callersTraceIdHexString, "N");

            var headers = new Dictionary<string, string>
            {
                ["traceparent"] = $"00-{callersTraceIdHexString}-b7ad6b7169203331-01"
            };

            // when
            HttpResponseMessage response = await this.apiBroker.GetResponseAsync("/", headers);

            // then
            // One id across every hop, which is the point of the convention. The caller named the
            // trace; this service files its audit rows and metric spans under the same value and
            // says so in the response.
            ReadCorrelationId(response).Should().Be(expectedCorrelationId);
        }

        [Fact]
        public async Task ShouldReturnADifferentCorrelationIdPerRequestAsync()
        {
            // given . when
            HttpResponseMessage firstResponse = await this.apiBroker.GetResponseAsync("/");
            HttpResponseMessage secondResponse = await this.apiBroker.GetResponseAsync("/");

            // then
            // One id per request, not one per host. A shared id would correlate nothing.
            ReadCorrelationId(firstResponse).Should().NotBe(ReadCorrelationId(secondResponse));
        }

        [Fact]
        public void ShouldSatisfyTheMetricLibrarysRequestSpanPort()
        {
            // given . when
            using IServiceScope scope =
                this.apiBroker.WebApplicationFactory.Services.CreateScope();

            var requestTraceBroker =
                scope.ServiceProvider.GetRequiredService<IRequestTraceBroker>();

            // then
            // Registered, and registered as the real implementation. The library falls back to a
            // null object when a host supplies nothing, so a missing registration here would not
            // fail anything - it would silently stop anchoring every metric span.
            requestTraceBroker.Should().BeOfType<RequestTraceBroker>();
        }

        private static Guid ReadCorrelationId(HttpResponseMessage response)
        {
            response.Headers
                .TryGetValues(CorrelationMiddleware.CorrelationIdHeaderName, out IEnumerable<string> values)
                    .Should().BeTrue(
                        $"every response should carry the {CorrelationMiddleware.CorrelationIdHeaderName} header");

            string rawCorrelationId = values.Should().ContainSingle().Subject;

            // ParseExact with "N" rather than TryParse: the header has to be the 32 hex
            // characters App Insights indexes operation_Id as and the caller sent in traceparent.
            // TryParse accepts the dashed form too, which is how the wrong format shipped.
            Guid.TryParseExact(rawCorrelationId, "N", out Guid correlationId).Should()
                .BeTrue($"'{rawCorrelationId}' should be a 32 character hex trace id");

            return correlationId;
        }
    }
}
