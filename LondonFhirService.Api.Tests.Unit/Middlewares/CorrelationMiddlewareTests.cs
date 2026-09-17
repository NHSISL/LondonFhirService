// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using LondonFhirService.Api.Middlewares;
using LondonFhirService.Core.Brokers.Correlations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Unit.Middlewares
{
    /// <summary>
    /// The consumer gets the correlation id back whatever the outcome. The point of drawing it
    /// in the pipeline rather than in the coordination service is the request that never reaches
    /// a controller - and that is the one they actually ring up about.
    /// </summary>
    public class CorrelationMiddlewareTests
    {
        private readonly Mock<ICorrelationBroker> correlationBrokerMock;

        public CorrelationMiddlewareTests() =>
            this.correlationBrokerMock = new Mock<ICorrelationBroker>();

        [Fact]
        public async Task ShouldReturnTheCorrelationIdInTheResponseHeaderAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            Guid expectedCorrelationId = randomCorrelationId;
            var responseFeature = new RecordingResponseFeature();
            HttpContext httpContext = CreateHttpContext(responseFeature);

            this.correlationBrokerMock.Setup(broker =>
                broker.GetCorrelationIdAsync())
                    .ReturnsAsync(expectedCorrelationId);

            var middleware = new CorrelationMiddleware(next: context => Task.CompletedTask);

            // when
            await middleware.InvokeAsync(httpContext, this.correlationBrokerMock.Object);
            await responseFeature.SendHeadersAsync();

            // then
            // The 32-hex form, asserted explicitly rather than via ToString() - the previous
            // assertion re-derived the value the same wrong way the code did, so it locked the
            // dashed form in instead of catching it.
            httpContext.Response.Headers[CorrelationMiddleware.CorrelationIdHeaderName]
                .ToString().Should().Be(expectedCorrelationId.ToString("N"));

            httpContext.Response.Headers[CorrelationMiddleware.CorrelationIdHeaderName]
                .ToString().Should().NotContain("-");

            this.correlationBrokerMock.Verify(broker =>
                broker.GetCorrelationIdAsync(),
                    Times.Once);

            this.correlationBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnTheCorrelationIdInTheResponseHeaderWhenTheRequestFailsAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            Guid expectedCorrelationId = randomCorrelationId;
            var responseFeature = new RecordingResponseFeature();
            HttpContext httpContext = CreateHttpContext(responseFeature);

            this.correlationBrokerMock.Setup(broker =>
                broker.GetCorrelationIdAsync())
                    .ReturnsAsync(expectedCorrelationId);

            // Stands in for authentication, authorization or the timeout policy short circuiting
            // the pipeline. None of them reach the controller, and all of them used to come back
            // with nothing to quote against the logs.
            var middleware = new CorrelationMiddleware(next: context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                return Task.CompletedTask;
            });

            // when
            await middleware.InvokeAsync(httpContext, this.correlationBrokerMock.Object);
            await responseFeature.SendHeadersAsync();

            // then
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

            // The 32-hex form, asserted explicitly rather than via ToString() - the previous
            // assertion re-derived the value the same wrong way the code did, so it locked the
            // dashed form in instead of catching it.
            httpContext.Response.Headers[CorrelationMiddleware.CorrelationIdHeaderName]
                .ToString().Should().Be(expectedCorrelationId.ToString("N"));

            httpContext.Response.Headers[CorrelationMiddleware.CorrelationIdHeaderName]
                .ToString().Should().NotContain("-");
        }

        [Fact]
        public async Task ShouldWriteTheHeaderOnlyWhenTheResponseStartsAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            var responseFeature = new RecordingResponseFeature();
            HttpContext httpContext = CreateHttpContext(responseFeature);

            this.correlationBrokerMock.Setup(broker =>
                broker.GetCorrelationIdAsync())
                    .ReturnsAsync(randomCorrelationId);

            var middleware = new CorrelationMiddleware(next: context => Task.CompletedTask);

            // when
            await middleware.InvokeAsync(httpContext, this.correlationBrokerMock.Object);

            // then
            // Deferred rather than written inline, so the header lands on whatever response is
            // eventually produced - including one written by a handler further down - instead of
            // only on the ones that come back through the middleware unchanged.
            httpContext.Response.Headers
                .Should().NotContainKey(CorrelationMiddleware.CorrelationIdHeaderName);
        }

        [Fact]
        public async Task ShouldCallTheRestOfThePipelineAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            var responseFeature = new RecordingResponseFeature();
            HttpContext httpContext = CreateHttpContext(responseFeature);
            bool nextWasCalled = false;

            this.correlationBrokerMock.Setup(broker =>
                broker.GetCorrelationIdAsync())
                    .ReturnsAsync(randomCorrelationId);

            var middleware = new CorrelationMiddleware(next: context =>
            {
                nextWasCalled = true;

                return Task.CompletedTask;
            });

            // when
            await middleware.InvokeAsync(httpContext, this.correlationBrokerMock.Object);

            // then
            nextWasCalled.Should().BeTrue();
        }

        private static HttpContext CreateHttpContext(IHttpResponseFeature responseFeature)
        {
            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(new HttpRequestFeature());
            features.Set(responseFeature);

            return new DefaultHttpContext(features);
        }

        /// <summary>
        /// The response feature DefaultHttpContext ships with treats OnStarting as a no op, so a
        /// callback registered against it can never be observed. This one keeps the callbacks and
        /// runs them on demand, which is what a real server does when it begins writing the
        /// response.
        /// </summary>
        private sealed class RecordingResponseFeature : IHttpResponseFeature
        {
            private readonly List<(Func<object, Task> Callback, object State)> onStartingCallbacks =
                new List<(Func<object, Task>, object)>();

            public int StatusCode { get; set; } = StatusCodes.Status200OK;
            public string ReasonPhrase { get; set; }
            public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
            public Stream Body { get; set; } = Stream.Null;
            public bool HasStarted { get; private set; }

            public void OnStarting(Func<object, Task> callback, object state) =>
                this.onStartingCallbacks.Add((callback, state));

            public void OnCompleted(Func<object, Task> callback, object state)
            { }

            public async Task SendHeadersAsync()
            {
                HasStarted = true;

                foreach ((Func<object, Task> callback, object state) in this.onStartingCallbacks)
                {
                    await callback(state);
                }
            }
        }
    }
}
