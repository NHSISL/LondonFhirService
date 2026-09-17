// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Metrics;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Brokers
{
    /// <summary>
    /// The telemetry side of a metric span only exists if something is subscribed to the
    /// ActivitySource - StartActivity returns null otherwise and the span is silently dropped.
    /// These tests subscribe a listener the way the host does, so the path is exercised rather
    /// than assumed.
    /// </summary>
    public class MetricBrokerTests : IDisposable
    {
        private readonly string activitySourceName;
        private readonly List<Activity> capturedActivities;
        private readonly ActivityListener activityListener;
        private readonly IMetricBroker metricBroker;

        public MetricBrokerTests()
        {
            // A name per test class instance, so a listener never sees another test's spans.
            this.activitySourceName = $"LondonFhirService.Metrics.Tests.{Guid.NewGuid():N}";
            this.capturedActivities = new List<Activity>();

            this.activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == this.activitySourceName,

                Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                    ActivitySamplingResult.AllDataAndRecorded,

                ActivityStopped = activity => this.capturedActivities.Add(activity)
            };

            ActivitySource.AddActivityListener(this.activityListener);

            this.metricBroker = new MetricBroker(new AuditAndMetricsConfigurations
            {
                ActivitySourceName = this.activitySourceName
            });
        }

        [Fact]
        public async Task ShouldPublishTheSpanToASubscribedListenerAsync()
        {
            // given
            IMetric metric = CreateMetric();

            // when
            await this.metricBroker.RecordAsync(metric, TestContext.Current.CancellationToken);

            // then
            // The configured source name is what the host subscribes to. If the broker ignored it
            // and used a hardcoded name, nothing here would fire.
            Activity activity = this.capturedActivities.Should().ContainSingle().Subject;
            activity.DisplayName.Should().Be($"{metric.Method}/{metric.Name}");
            activity.GetTagItem("metric.id").Should().Be(metric.Id.ToString());
            activity.GetTagItem("metric.type").Should().Be(metric.Type.ToString());
            activity.GetTagItem("metric.target").Should().Be(metric.Target);
        }

        [Fact]
        public async Task ShouldReportTheMeasuredDurationRatherThanTheReplayDurationAsync()
        {
            // given
            IMetric metric = CreateMetric();

            // Started is now, not 42 seconds ago. Backdating it would let Activity.Stop()'s own
            // wall-clock fallback produce the same 42 seconds the assertion expects, and the test
            // would pass with SetEndTime removed - pinning nothing.
            metric.Started = DateTimeOffset.UtcNow;
            metric.DurationMs = 42_000;
            metric.Completed = metric.Started.AddMilliseconds(metric.DurationMs);

            // when
            await this.metricBroker.RecordAsync(metric, TestContext.Current.CancellationToken);

            // then
            // The span is replayed after the fact, so without an explicit end time it would
            // report the microseconds this method took instead of the 42 seconds it measured.
            Activity activity = this.capturedActivities.Should().ContainSingle().Subject;
            activity.Duration.TotalMilliseconds.Should().BeApproximately(metric.DurationMs, precision: 50);
        }

        [Fact]
        public async Task ShouldGroupEverySpanOfOneRequestUnderOneTraceAsync()
        {
            // given
            Guid correlationId = Guid.NewGuid();
            IMetric first = CreateMetric();
            IMetric second = CreateMetric();
            first.CorrelationId = correlationId;
            second.CorrelationId = correlationId;

            // when
            await this.metricBroker.RecordAsync(
                new List<IMetric> { first, second }, TestContext.Current.CancellationToken);

            // then
            // The trace id is derived from the correlation id, which is what puts every span of
            // one request under a single operation in the telemetry viewer.
            this.capturedActivities.Should().HaveCount(2);

            this.capturedActivities.Select(activity => activity.TraceId.ToHexString())
                .Distinct()
                .Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldReplayTheSpanUnderTheTraceTheCorrelationIdNamesAsync()
        {
            // given
            Guid correlationId = Guid.NewGuid();
            IMetric metric = CreateMetric();
            metric.CorrelationId = correlationId;

            // when
            await this.metricBroker.RecordAsync(
                new List<IMetric> { metric }, TestContext.Current.CancellationToken);

            // then
            // A correlation id is a W3C trace id: the host takes it from the request's Activity,
            // which carries whatever the caller sent in traceparent. Rebuilding it here has to be
            // exact, or a replayed span lands under an operation of its own instead of under the
            // caller's trace. Deriving it from Guid's bytes did exactly that - Guid stores its
            // first three fields little endian and a trace id is not stored that way at all.
            Activity activity = this.capturedActivities.Should().ContainSingle().Subject;
            activity.TraceId.ToHexString().Should().Be(correlationId.ToString("N"));
        }

        [Fact]
        public async Task ShouldAnchorTheSpanUnderTheRequestItBelongsToAsync()
        {
            // given
            string requestSpanId = ActivitySpanId.CreateRandom().ToHexString();
            IMetric metric = CreateMetric();
            metric.RequestSpanId = requestSpanId;

            // when
            await this.metricBroker.RecordAsync(metric, TestContext.Current.CancellationToken);

            // then
            // The whole flat group hangs off the HTTP request's own span, so the metrics appear
            // under the request in the transaction view instead of floating beside it. Flattening
            // and anchoring are independent - the siblings stay siblings, they just now hang from
            // something that exists.
            Activity activity = this.capturedActivities.Should().ContainSingle().Subject;
            activity.ParentSpanId.ToHexString().Should().Be(requestSpanId);
        }

        [Fact]
        public async Task ShouldGiveEverySpanOfOneRequestTheSameParentAsync()
        {
            // given
            string requestSpanId = ActivitySpanId.CreateRandom().ToHexString();
            Guid correlationId = Guid.NewGuid();
            IMetric first = CreateMetric();
            IMetric second = CreateMetric();
            first.CorrelationId = correlationId;
            second.CorrelationId = correlationId;
            first.RequestSpanId = requestSpanId;
            second.RequestSpanId = requestSpanId;

            // when
            await this.metricBroker.RecordAsync(
                new List<IMetric> { first, second },
                TestContext.Current.CancellationToken);

            // then
            // Deliberately flat: reproducing the real tree made the metric view too noisy to
            // read. The nesting lives in the metric.id and metric.parentId tags instead.
            this.capturedActivities.Select(activity => activity.ParentSpanId.ToHexString())
                .Distinct()
                .Should().ContainSingle().Subject.Should().Be(requestSpanId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-span-id")]
        [InlineData("zzzzzzzzzzzzzzzz")]
        public async Task ShouldFallBackToTheDerivedParentWhenThereIsNoUsableRequestSpanAsync(
            string unusableRequestSpanId)
        {
            // given
            IMetric metric = CreateMetric();
            metric.RequestSpanId = unusableRequestSpanId;

            // when
            await this.metricBroker.RecordAsync(metric, TestContext.Current.CancellationToken);

            // then
            // A background worker has no request span, and a malformed one must not take down the
            // write path this rides along with. Either way the span still joins the right trace.
            Activity activity = this.capturedActivities.Should().ContainSingle().Subject;
            activity.TraceId.ToHexString().Should().Be(metric.CorrelationId.ToString("N"));
            activity.ParentSpanId.ToHexString().Should().NotBe("0000000000000000");
        }

        [Fact]
        public async Task ShouldAnchorEachMetricOfAMixedBatchUnderItsOwnRequestAsync()
        {
            // given
            string firstRequestSpanId = ActivitySpanId.CreateRandom().ToHexString();
            string secondRequestSpanId = ActivitySpanId.CreateRandom().ToHexString();
            IMetric first = CreateMetric();
            IMetric second = CreateMetric();
            first.RequestSpanId = firstRequestSpanId;
            second.RequestSpanId = secondRequestSpanId;

            // when
            await this.metricBroker.RecordAsync(
                new List<IMetric> { first, second },
                TestContext.Current.CancellationToken);

            // then
            // The span id rides on each metric rather than arriving as one parameter for the
            // batch. A single parameter stamped one request's span id onto spans belonging to a
            // different trace, where that parent does not exist and the span disappears from the
            // transaction view instead of grouping.
            this.capturedActivities.Should().HaveCount(2);

            this.capturedActivities.Select(activity => activity.ParentSpanId.ToHexString())
                .Should().BeEquivalentTo(new[] { firstRequestSpanId, secondRequestSpanId });
        }

        [Fact]
        public async Task ShouldMarkAFailedSpanAsErrorAsync()
        {
            // given
            IMetric metric = CreateMetric();
            metric.Status = MetricStatus.Failed;
            metric.ErrorCode = "ProviderTimeout";

            // when
            await this.metricBroker.RecordAsync(metric, TestContext.Current.CancellationToken);

            // then
            Activity activity = this.capturedActivities.Should().ContainSingle().Subject;
            activity.Status.Should().Be(ActivityStatusCode.Error);
            activity.StatusDescription.Should().Be("ProviderTimeout");
        }

        private static IMetric CreateMetric()
        {
            DateTimeOffset started = DateTimeOffset.UtcNow.AddSeconds(-1);

            return new TestMetric
            {
                Id = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                Method = new MnemonicString(wordCount: 2).GetValue(),
                Name = new MnemonicString(wordCount: 2).GetValue(),
                Target = new MnemonicString(wordCount: 2).GetValue(),
                Type = MetricType.Provider,
                Status = MetricStatus.Succeeded,
                Started = started,
                Completed = started.AddMilliseconds(1000),
                DurationMs = 1000
            };
        }

        public void Dispose()
        {
            this.activityListener.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
