// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using LondonFhirService.Core.Brokers.Correlations;
using LondonFhirService.Core.Brokers.Identifiers;
using Microsoft.AspNetCore.Http;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Brokers.Correlations
{
    /// <summary>
    /// One id per request, whoever asks for it and however often - and where that id comes from.
    /// The header the consumer is handed, the id the audit rows are written under and the trace
    /// the telemetry groups on are only the same thing if these hold.
    /// </summary>
    public class CorrelationBrokerTests : IDisposable
    {
        private readonly Mock<IHttpContextAccessor> httpContextAccessorMock;
        private readonly Mock<IIdentifierBroker> identifierBrokerMock;
        private readonly ICorrelationBroker correlationBroker;
        private readonly Activity previousActivity;

        public CorrelationBrokerTests()
        {
            this.httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            this.identifierBrokerMock = new Mock<IIdentifierBroker>();

            this.correlationBroker = new CorrelationBroker(
                httpContextAccessor: this.httpContextAccessorMock.Object,
                identifierBroker: this.identifierBrokerMock.Object);

            // Activity.Current is ambient and every test here depends on what it is, so each one
            // starts from a known empty state and puts back whatever it found.
            this.previousActivity = Activity.Current;
            Activity.Current = null;
        }

        public void Dispose() =>
            Activity.Current = this.previousActivity;

        [Fact]
        public async Task ShouldTakeTheCorrelationIdFromTheRequestTraceAsync()
        {
            // given
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);

            using Activity activity = StartW3CActivity();
            Guid expectedCorrelationId = Guid.ParseExact(activity.TraceId.ToHexString(), "N");

            // when
            Guid actualCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            // The correlation id IS the trace id - same 128 bits, same 32 hex characters - so an
            // id off a response header or an audit row pastes straight into the telemetry viewer.
            actualCorrelationId.Should().Be(expectedCorrelationId);
            actualCorrelationId.ToString("N").Should().Be(activity.TraceId.ToHexString());

            items.Should().ContainKey(CorrelationBroker.RequestCorrelationItemKey)
                .WhoseValue.Should().BeOfType<CorrelationBroker.RequestCorrelation>()
                    .Which.CorrelationId.Should().Be(expectedCorrelationId);

            // Nothing was minted: the id already existed, it just was not ours.
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAdoptTheTraceTheCallerSentAsync()
        {
            // given
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);

            // A traceparent as a gateway or upstream caller would send it.
            string callersTraceIdHexString = "0af7651916cd43dd8448eb211c80319c";

            using Activity activity = StartW3CActivity(
                parentId: "00-" + callersTraceIdHexString + "-b7ad6b7169203331-01");

            Guid expectedCorrelationId = Guid.ParseExact(callersTraceIdHexString, "N");

            // when
            Guid actualCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            // The whole point of the convention: one id spans every hop. Minting our own here
            // would leave two ids for one request and no way to join them.
            actualCorrelationId.Should().Be(expectedCorrelationId);
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnTheSameCorrelationIdForEveryReadWithinTheRequestAsync()
        {
            // given
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);
            using Activity activity = StartW3CActivity();

            // when
            Guid firstCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();
            Guid secondCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            // The middleware reads it to write the header and the exposer reads it to hand it
            // down. A second id here would mean the consumer holds one value and the audit trail
            // records another.
            firstCorrelationId.Should().Be(secondCorrelationId);
        }

        [Fact]
        public async Task ShouldReturnTheCorrelationIdAlreadyOnTheContextAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            Guid expectedCorrelationId = randomCorrelationId;

            var items = new Dictionary<object, object>
            {
                [CorrelationBroker.RequestCorrelationItemKey] =
                    new CorrelationBroker.RequestCorrelation(expectedCorrelationId, null)
            };

            CreateHttpContext(items);

            // An activity is running and is deliberately NOT the source here: something already
            // settled this request's id, and changing it mid-request would split the trail.
            using Activity activity = StartW3CActivity();

            // when
            Guid actualCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            actualCorrelationId.Should().Be(expectedCorrelationId);
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMintACorrelationIdWhenThereIsNoTraceToReadAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            Guid expectedCorrelationId = randomCorrelationId;
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(expectedCorrelationId);

            // when
            // Activity.Current is null - nothing is listening for activities, which is the case
            // in a bare test host.
            Guid actualCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            actualCorrelationId.Should().Be(expectedCorrelationId);

            items.Should().ContainKey(CorrelationBroker.RequestCorrelationItemKey)
                .WhoseValue.Should().BeOfType<CorrelationBroker.RequestCorrelation>()
                    .Which.CorrelationId.Should().Be(expectedCorrelationId);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMintACorrelationIdWhenTheActivityIsNotW3CAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            Guid expectedCorrelationId = randomCorrelationId;
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);

            using var activity = new Activity("hierarchical");
            activity.SetIdFormat(ActivityIdFormat.Hierarchical);
            activity.Start();

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(expectedCorrelationId);

            // when
            Guid actualCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            // A hierarchical activity has no trace id to read. Falling through to a minted id
            // keeps the request correlated even though it cannot join the caller's trace.
            actualCorrelationId.Should().Be(expectedCorrelationId);
        }

        [Fact]
        public async Task ShouldCaptureTheRequestSpanIdFromTheSameActivityAsync()
        {
            // given
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);
            using Activity activity = StartW3CActivity();

            // when
            Guid correlationId = await this.correlationBroker.GetCorrelationIdAsync();
            string actualRequestSpanId = await this.correlationBroker.GetRequestSpanIdAsync();

            // then
            // One activity, both values. They have to describe the same trace or the metric spans
            // would be anchored under a span belonging to a different request.
            actualRequestSpanId.Should().Be(activity.SpanId.ToHexString());
            correlationId.ToString("N").Should().Be(activity.TraceId.ToHexString());

            // Resolution happens once however it is entered, so the identifier broker is never
            // asked for an id that the trace already supplied.
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldCaptureTheRequestSpanIdWhenAskedForItFirstAsync()
        {
            // given
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);
            using Activity activity = StartW3CActivity();

            // when
            // Reversed order: nothing guarantees the correlation id is asked for first, and
            // resolving from either entry point has to capture both.
            string actualRequestSpanId = await this.correlationBroker.GetRequestSpanIdAsync();
            Guid correlationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            actualRequestSpanId.Should().Be(activity.SpanId.ToHexString());
            correlationId.ToString("N").Should().Be(activity.TraceId.ToHexString());
        }

        [Fact]
        public async Task ShouldReturnTheSameRequestSpanIdForEveryReadAsync()
        {
            // given
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);
            using Activity activity = StartW3CActivity();

            // when
            string firstRequestSpanId = await this.correlationBroker.GetRequestSpanIdAsync();
            string secondRequestSpanId = await this.correlationBroker.GetRequestSpanIdAsync();

            // then
            // Settled once and kept on the context. A later read from inside a child activity
            // must not capture that child's span id in place of the request's.
            firstRequestSpanId.Should().Be(secondRequestSpanId);
            firstRequestSpanId.Should().Be(activity.SpanId.ToHexString());
        }

        [Fact]
        public async Task ShouldReturnNoRequestSpanIdWhenThereIsNoTraceToReadAsync()
        {
            // given
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(Guid.NewGuid());

            // when
            // Activity.Current is null, so there is no request span to name.
            string actualRequestSpanId = await this.correlationBroker.GetRequestSpanIdAsync();

            // then
            // Null rather than an invented value: the metric broker reads this as "no request"
            // and falls back to the parent it derives from the correlation id.
            actualRequestSpanId.Should().BeNull();
        }

        [Fact]
        public async Task ShouldReturnNoRequestSpanIdWhenThereIsNoHttpContextAsync()
        {
            // given
            this.httpContextAccessorMock.SetupGet(accessor =>
                accessor.HttpContext)
                    .Returns((HttpContext)null);

            // when
            string actualRequestSpanId = await this.correlationBroker.GetRequestSpanIdAsync();

            // then
            actualRequestSpanId.Should().BeNull();
        }

        [Fact]
        public async Task ShouldMintACorrelationIdWhenThereIsNoHttpContextAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            Guid expectedCorrelationId = randomCorrelationId;

            this.httpContextAccessorMock.SetupGet(accessor =>
                accessor.HttpContext)
                    .Returns((HttpContext)null);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(expectedCorrelationId);

            // when
            Guid actualCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            // A background worker has no request behind it. Guid.Empty would validate away at
            // the next layer and stamp every row it writes with the same meaningless value.
            actualCorrelationId.Should().Be(expectedCorrelationId);
            actualCorrelationId.Should().NotBe(Guid.Empty);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotResettleTheCorrelationIdWhenTheSpanIdIsReadFirstAsync()
        {
            // given
            var items = new Dictionary<object, object>();
            CreateHttpContext(items);
            using Activity activity = StartW3CActivity();

            // when
            // Deliberately the span id first. The earlier two-key form missed on the span key,
            // re-resolved, and overwrote an already settled correlation id - so the middleware
            // handed the consumer one value in the header while every audit row and metric span
            // afterwards carried another.
            string requestSpanId = await this.correlationBroker.GetRequestSpanIdAsync();
            Guid firstCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();
            Guid secondCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            requestSpanId.Should().Be(activity.SpanId.ToHexString());
            firstCorrelationId.Should().Be(secondCorrelationId);
            firstCorrelationId.ToString("N").Should().Be(activity.TraceId.ToHexString());
        }

        [Fact]
        public async Task ShouldNotOverwriteACorrelationIdThatIsAlreadySettledAsync()
        {
            // given
            Guid settledCorrelationId = Guid.NewGuid();

            var items = new Dictionary<object, object>
            {
                [CorrelationBroker.RequestCorrelationItemKey] =
                    new CorrelationBroker.RequestCorrelation(settledCorrelationId, "b7ad6b7169203331")
            };

            CreateHttpContext(items);

            // A different activity is running, so re-resolving would visibly change the answer.
            using Activity activity = StartW3CActivity();

            // when
            string requestSpanId = await this.correlationBroker.GetRequestSpanIdAsync();
            Guid actualCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            actualCorrelationId.Should().Be(settledCorrelationId);
            requestSpanId.Should().Be("b7ad6b7169203331");
            actualCorrelationId.ToString("N").Should().NotBe(activity.TraceId.ToHexString());
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnTheSameMintedCorrelationIdWhenThereIsNoHttpContextAsync()
        {
            // given
            Guid randomCorrelationId = Guid.NewGuid();
            Guid expectedCorrelationId = randomCorrelationId;

            this.httpContextAccessorMock.SetupGet(accessor =>
                accessor.HttpContext)
                    .Returns((HttpContext)null);

            this.identifierBrokerMock.SetupSequence(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(expectedCorrelationId)
                    .ReturnsAsync(Guid.NewGuid());

            // when
            Guid firstCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();
            Guid secondCorrelationId = await this.correlationBroker.GetCorrelationIdAsync();

            // then
            // A background worker has no HttpContext to hang the answer off, so the broker holds
            // it in a field instead. Minting per call split one logical operation across two
            // traces, with the audit rows under one id and the metric rows under another.
            firstCorrelationId.Should().Be(expectedCorrelationId);
            secondCorrelationId.Should().Be(expectedCorrelationId);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        private static Activity StartW3CActivity(string parentId = null)
        {
            var activity = new Activity("request");
            activity.SetIdFormat(ActivityIdFormat.W3C);

            if (parentId is not null)
            {
                activity.SetParentId(parentId);
            }

            return activity.Start();
        }

        private void CreateHttpContext(IDictionary<object, object> items)
        {
            var httpContextMock = new Mock<HttpContext>();

            httpContextMock.SetupGet(httpContext =>
                httpContext.Items)
                    .Returns(items);

            this.httpContextAccessorMock.SetupGet(accessor =>
                accessor.HttpContext)
                    .Returns(httpContextMock.Object);
        }
    }
}
