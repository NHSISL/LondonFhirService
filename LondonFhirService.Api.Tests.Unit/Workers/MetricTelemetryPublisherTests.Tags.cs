// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Diagnostics;
using FluentAssertions;
using LondonFhirService.Api.Workers;
using Microsoft.ApplicationInsights.DataContracts;

namespace LondonFhirService.Api.Tests.Unit.Workers
{
    /// <summary>
    /// What actually reaches Application Insights. A metric span carries numeric tags as well as
    /// string ones, and the two obvious ways of enumerating them do not agree: Activity.Tags is a
    /// string-typed projection that drops everything else without complaint.
    /// </summary>
    public class MetricTelemetryPublisherTagsTests : IDisposable
    {
        private readonly ActivitySource activitySource;
        private readonly ActivityListener activityListener;

        public MetricTelemetryPublisherTagsTests()
        {
            string sourceName = $"LondonFhirService.Metrics.Tests.{Guid.NewGuid():N}";
            this.activitySource = new ActivitySource(sourceName);

            this.activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == sourceName,

                Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                    ActivitySamplingResult.AllDataAndRecorded
            };

            ActivitySource.AddActivityListener(this.activityListener);
        }

        [Fact]
        public void ShouldCarryNumericSpanTagsIntoTheTelemetryProperties()
        {
            // given
            using Activity activity = this.activitySource.StartActivity("Method/Name");
            activity.SetTag("metric.name", "DDS Provider");
            activity.SetTag("metric.durationMs", 1234.5d);
            activity.SetTag("metric.payloadBytes", 98765L);
            activity.Stop();

            // when
            DependencyTelemetry dependencyTelemetry =
                MetricTelemetryPublisher.CreateDependencyTelemetry(activity);

            // then
            // Enumerating Activity.Tags instead of TagObjects drops these two silently - they are
            // the only non-string tags a span carries, and the two a dashboard most wants.
            dependencyTelemetry.Properties.Should().ContainKey("metric.durationMs");
            dependencyTelemetry.Properties.Should().ContainKey("metric.payloadBytes");
            dependencyTelemetry.Properties["metric.durationMs"].Should().Be("1234.5");
            dependencyTelemetry.Properties["metric.payloadBytes"].Should().Be("98765");
            dependencyTelemetry.Properties["metric.name"].Should().Be("DDS Provider");
        }

        [Fact]
        public void ShouldFormatNumbersInvariantlyRegardlessOfTheHostsCulture()
        {
            // given
            System.Globalization.CultureInfo previousCulture =
                System.Threading.Thread.CurrentThread.CurrentCulture;

            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo("de-DE");

            try
            {
                using Activity activity = this.activitySource.StartActivity("Method/Name");
                activity.SetTag("metric.durationMs", 1234.5d);
                activity.Stop();

                // when
                DependencyTelemetry dependencyTelemetry =
                    MetricTelemetryPublisher.CreateDependencyTelemetry(activity);

                // then
                // A German host would otherwise write "1234,5" and every duration would be
                // unparseable by whatever reads the dashboard.
                dependencyTelemetry.Properties["metric.durationMs"].Should().Be("1234.5");
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = previousCulture;
            }
        }

        [Fact]
        public void ShouldReportTheSpansOwnDurationAndOutcome()
        {
            // given
            using Activity activity = this.activitySource.StartActivity("Method/Name");
            activity.SetTag("metric.type", "Provider");
            activity.SetTag("metric.target", "Nhs.Dds.Stu3");
            activity.SetTag("metric.errorCode", "ProviderTimeout");
            activity.SetStatus(ActivityStatusCode.Error, "ProviderTimeout");
            activity.SetEndTime(activity.StartTimeUtc.AddSeconds(9));
            activity.Stop();

            // when
            DependencyTelemetry dependencyTelemetry =
                MetricTelemetryPublisher.CreateDependencyTelemetry(activity);

            // then
            dependencyTelemetry.Type.Should().Be("Provider");
            dependencyTelemetry.Target.Should().Be("Nhs.Dds.Stu3");
            dependencyTelemetry.ResultCode.Should().Be("ProviderTimeout");
            dependencyTelemetry.Success.Should().BeFalse();
            dependencyTelemetry.Duration.Should().Be(TimeSpan.FromSeconds(9));
        }

        public void Dispose()
        {
            this.activityListener.Dispose();
            this.activitySource.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
