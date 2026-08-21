// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            await this.metricBroker.RecordAsync(metric);

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
            metric.Started = DateTimeOffset.UtcNow.AddSeconds(-42);
            metric.DurationMs = 42_000;
            metric.Completed = metric.Started.AddMilliseconds(metric.DurationMs);

            // when
            await this.metricBroker.RecordAsync(metric);

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
            await this.metricBroker.RecordAsync(new List<IMetric> { first, second });

            // then
            // The trace id is derived from the correlation id, which is what puts every span of
            // one request under a single operation in the telemetry viewer.
            this.capturedActivities.Should().HaveCount(2);

            this.capturedActivities.Select(activity => activity.TraceId.ToHexString())
                .Distinct()
                .Should().ContainSingle();
        }

        [Fact]
        public async Task ShouldMarkAFailedSpanAsErrorAsync()
        {
            // given
            IMetric metric = CreateMetric();
            metric.Status = MetricStatus.Failed;
            metric.ErrorCode = "ProviderTimeout";

            // when
            await this.metricBroker.RecordAsync(metric);

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
